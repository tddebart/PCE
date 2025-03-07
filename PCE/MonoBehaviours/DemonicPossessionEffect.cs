﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnboundLib;
using HarmonyLib;
using System.Reflection;
using Photon.Pun;
using System.Linq;
using PCE.Utils;
using System.Collections.ObjectModel;
using ModdingUtils.MonoBehaviours;
using UnboundLib.Utils;

namespace PCE.MonoBehaviours
{
    /*
        List of effects:
            1. Extra vibrate
 
     */

    public class DemonicPossessionEffect : MonoBehaviour
    {
        private readonly float maxDuration = 20f;
        private readonly float minDuration = 5f;

        private Player player;
        private Gun gun;
        private CharacterData data;
        private HealthHandler health;
        private Gravity gravity;
        private Block block;
        private GunAmmo gunAmmo;
        private CharacterStatModifiers statModifiers;


        private readonly System.Random rng = new System.Random();

        internal float xshakemag = 0.04f;
        internal float yshakemag = 0.02f;

        private List<Func<Player, Gun, GunAmmo, CharacterData, HealthHandler, Gravity, Block, CharacterStatModifiers, List<MonoBehaviour>>> effectFuncs = new List<Func<Player, Gun, GunAmmo, CharacterData, HealthHandler, Gravity, Block, CharacterStatModifiers, List<MonoBehaviour>>>();
        private List<MonoBehaviour> currentEffects;
        private int effectID;
        private float effectDuration = 0f;
        private float timeOfLastEffect;
        private bool ready = false;
        private readonly int framesToWait = 5;
        private int framesWaited = 0;

        void Awake()
        {
            this.player = gameObject.GetComponent<Player>();
            this.gun = this.player.GetComponent<Holding>().holdable.GetComponent<Gun>();
            this.data = this.player.GetComponent<CharacterData>();
            this.health = this.player.GetComponent<HealthHandler>();
            this.gravity = this.player.GetComponent<Gravity>();
            this.block = this.player.GetComponent<Block>();
            this.gunAmmo = this.gun.GetComponentInChildren<GunAmmo>();
            this.statModifiers = this.player.GetComponent<CharacterStatModifiers>();

            
            this.effectFuncs.Add(this.Effect_NullEffect);
            
            this.effectFuncs.Add(this.Effect_NoGravityEffect);
            this.effectFuncs.Add(this.Effect_InvisibleEffect);
            this.effectFuncs.Add(this.Effect_ShakeEffect);
            this.effectFuncs.Add(this.Effect_PopEffect);
            this.effectFuncs.Add(this.Effect_NukeEffect);
            
            this.effectFuncs.Add(this.Effect_BulletSpeedEffect);
            this.effectFuncs.Add(this.Effect_BulletDamageEffect);
            this.effectFuncs.Add(this.Effect_BulletBounceEffect);
            this.effectFuncs.Add(this.Effect_MovementSpeed);

            this.effectFuncs.Add(this.Effect_RainEffect);
            this.effectFuncs.Add(this.Effect_WallEffect);
            

        }

        void Start()
        {
            this.ready = false;
            this.timeOfLastEffect = -1f;
            this.effectDuration = -1f;
        }

        void Update()
        {
            // if the player is dead or otherwise not active (i.e. simulated) then clear all effects
            if (this.player.data.dead || !(bool)Traverse.Create(this.player.data.playerVel).Field("simulated").GetValue())
            {
                if (PhotonNetwork.OfflineMode)
                {
                    // offline mode
                    this.RPCA_ClearEffects();
                }
                else if (base.GetComponent<PhotonView>().IsMine)
                {
                    base.GetComponent<PhotonView>().RPC("RPCA_ClearEffects", RpcTarget.All, new object[] { });
                }
                return;
            }
            // get and apply a new effect if things are ready
            else if (this.ready)
            {
                this.ready = false;
                this.GetNewEffect();
                this.GetNewDuration();
                if (PhotonNetwork.OfflineMode)
                {
                    // offline mode
                    this.RPCA_ApplyCurrentEffect();
                }
                else if (base.GetComponent<PhotonView>().IsMine)
                {
                    base.GetComponent<PhotonView>().RPC("RPCA_ApplyCurrentEffect", RpcTarget.All, new object[] { });
                }
            }
            // if the duration of the effect has passed, clear all effects
            else if (Time.time >= this.timeOfLastEffect + this.effectDuration)
            {
                if (PhotonNetwork.OfflineMode)
                {
                    // offline mode
                    this.RPCA_ClearEffects();
                }
                else if (base.GetComponent<PhotonView>().IsMine)
                {
                    base.GetComponent<PhotonView>().RPC("RPCA_ClearEffects", RpcTarget.All, new object[] { });
                }
            }

            float rx = (float)this.rng.NextGaussianDouble();
            float ry = (float)this.rng.NextGaussianDouble();

            Vector3 position = new Vector3(xshakemag*rx, yshakemag*ry, 0.0f);

            this.player.transform.position += position;

        }
        public void OnDestroy()
        {
            if (PhotonNetwork.OfflineMode)
            {
                // offline mode
                this.RPCA_ClearEffects();
            }
            else if (base.GetComponent<PhotonView>().IsMine)
            {
                base.GetComponent<PhotonView>().RPC("RPCA_ClearEffects", RpcTarget.All, new object[] { });
            }
        }
        public void GetNewDuration()
        {
            float newEffectDuration = (this.maxDuration-this.minDuration)*(float)this.rng.NextDouble() + this.minDuration;

            if (PhotonNetwork.OfflineMode)
            {
                // offline mode
                this.effectDuration = newEffectDuration;
            }
            else if (base.GetComponent<PhotonView>().IsMine)
            {
                base.GetComponent<PhotonView>().RPC("RPCA_SetNewDuration", RpcTarget.All, new object[] { newEffectDuration });
            }
        }
        public void GetNewEffect()
        {
            int newEffectID = this.rng.Next(0, this.effectFuncs.Count);

            if (PhotonNetwork.OfflineMode)
            {
                // offline mode
                this.effectID = newEffectID;
            }
            else if (base.GetComponent<PhotonView>().IsMine)
            {
                base.GetComponent<PhotonView>().RPC("RPCA_SetNewEffectID", RpcTarget.All, new object[] { newEffectID });
            }
        }
       
        public void ResetEffectTimer()
        {
            this.timeOfLastEffect = Time.time;
        }
        public void Destroy()
        {
            UnityEngine.Object.Destroy(this);
        }

        public List<MonoBehaviour> Effect_NoGravityEffect(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            GravityEffect effect = player.gameObject.AddComponent<GravityEffect>();
            effect.SetGravityForceMultiplier(0f);
            return new List<MonoBehaviour> { effect };
        }
        public List<MonoBehaviour> Effect_InvisibleEffect(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            ColorEffect effect = player.gameObject.AddComponent<ColorEffect>();
            effect.SetColor(Color.clear);
            return new List<MonoBehaviour> { effect };
        }
        public List<MonoBehaviour> Effect_ShakeEffect(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            DemonicPossessionShakeEffect effect = player.gameObject.GetOrAddComponent<DemonicPossessionShakeEffect>();
            effect.SetXMagMult(10f);
            effect.SetYMagMult(10f);
            return new List<MonoBehaviour> { effect };
        }
        public List<MonoBehaviour> Effect_NullEffect(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            return new List<MonoBehaviour>();
        }
        public List<MonoBehaviour> Effect_PopEffect(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            PopEffect effect = player.gameObject.GetOrAddComponent<PopEffect>();
            effect.SetPeriod(3f);
            effect.SetSpacing(5f);
            return new List<MonoBehaviour> { effect };
        }
        public List<MonoBehaviour> Effect_NukeEffect(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            Gun newGun = this.gameObject.AddComponent<NukeGun>();

            SpawnBulletsEffect effect = player.gameObject.AddComponent<SpawnBulletsEffect>();
            effect.SetDirection(new Vector3(0f, 1f, 0f));
            effect.SetPosition(new Vector3(0f, 100f, 0f));
            effect.SetNumBullets(1);

            SpawnBulletsEffect.CopyGunStats(gun, newGun);

            newGun.damage = 1000f;
            newGun.reloadTime = float.MaxValue;
            newGun.ammo = 1;
            newGun.projectileSpeed = 2f;
            newGun.projectielSimulatonSpeed = 2f;
            newGun.explodeNearEnemyRange = 5f;
            newGun.explodeNearEnemyDamage = 1000f;
            newGun.projectileSize = 100f;
            newGun.projectileColor = Color.red;
            newGun.spread = 0f;
            newGun.multiplySpread = 0f;
            newGun.damageAfterDistanceMultiplier = 1f;
            newGun.objectsToSpawn = new ObjectsToSpawn[] { PreventRecursion.stopRecursionObjectToSpawn };

            effect.SetGun(newGun);

            ColorFlash thisColorFlash = this.player.gameObject.GetOrAddComponent<ColorFlash>();
            thisColorFlash.SetNumberOfFlashes(10);
            thisColorFlash.SetDuration(0.15f);
            thisColorFlash.SetDelayBetweenFlashes(0.15f);
            thisColorFlash.SetColor(Color.red);

            return new List<MonoBehaviour> { effect };
        }
        public List<MonoBehaviour> Effect_BulletSpeedEffect(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            // increase bullet speed, decrease player speed

            ReversibleEffect effect = this.gameObject.AddComponent<ReversibleEffect>();
            effect.gunStatModifier.projectileSpeed_mult = 2f;
            effect.gunStatModifier.projectielSimulatonSpeed_mult = 2f;
            effect.gunStatModifier.projectileColor = Color.cyan;

            effect.characterStatModifiersModifier.movementSpeed_mult = 0.5f;

            return new List<MonoBehaviour> { effect };
        }
        public List<MonoBehaviour> Effect_BulletDamageEffect(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            // increase bullet damage, take 50% of current health as damage

            ReversibleEffect effect = this.gameObject.AddComponent<ReversibleEffect>();
            
            effect.gunStatModifier.damage_mult = 2f;
            effect.gunStatModifier.projectileSize_mult = 2f;
            effect.gunStatModifier.projectileColor = Color.red;

            health.TakeDamage(new Vector2(0f, -0.5f * data.health), player.transform.position, Color.red, null, null, false, true);
            
            return new List<MonoBehaviour> { effect };
        }
        public List<MonoBehaviour> Effect_BulletBounceEffect(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            
            // bullets can bounce a bunch

            ReversibleEffect effect = this.gameObject.AddComponent<ReversibleEffect>();
            effect.gunStatModifier.reflects_add = 1000;
            effect.gunStatModifier.speedMOnBounce_mult = 0f;
            effect.gunStatModifier.speedMOnBounce_add = 1.02f;
            effect.gunStatModifier.dmgMOnBounce_mult = 0f;
            effect.gunStatModifier.dmgMOnBounce_add = 0.95f;
            effect.gunStatModifier.destroyBulletAfter_mult = 0f;
            effect.gunStatModifier.destroyBulletAfter_add = 1000000f;
            effect.gunStatModifier.projectileColor = Color.magenta;

            // get the screenEdge (with screenEdgeBounce component) from the TargetBounce card
            List<CardInfo> activecards = ((ObservableCollection<CardInfo>)typeof(CardManager).GetField("activeCards", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null)).ToList();
            List<CardInfo> inactivecards = (List<CardInfo>)typeof(CardManager).GetField("inactiveCards", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
            List<CardInfo> allcards = activecards.Concat(inactivecards).ToList();

            CardInfo targetBounceCard = allcards.Where(card => card.gameObject.name == "TargetBounce").ToList()[0];
            Gun targetBounceGun = targetBounceCard.GetComponent<Gun>();
            ObjectsToSpawn screenEdgeToSpawn = (new List<ObjectsToSpawn>(targetBounceGun.objectsToSpawn)).Where(objectToSpawn => objectToSpawn.AddToProjectile.GetComponent<ScreenEdgeBounce>() != null).ToList()[0];

            effect.gunStatModifier.objectsToSpawn_add = new List<ObjectsToSpawn> { screenEdgeToSpawn };

            return new List<MonoBehaviour> { effect };
        }
        public List<MonoBehaviour> Effect_MovementSpeed(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            
            // way too much movement speed, much lower attack speed and much lower projectile speed

            ReversibleEffect effect = player.gameObject.AddComponent<ReversibleEffect>();
            effect.characterStatModifiersModifier.movementSpeed_mult = 6f;
            effect.characterStatModifiersModifier.jump_mult = 2f;
            effect.gunStatModifier.attackSpeedMultiplier_mult = 2f;
            effect.gunStatModifier.projectileSpeed_mult = 0.5f;

            return new List<MonoBehaviour> { effect };
        }
        public List<MonoBehaviour> Effect_RainEffect(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            Gun newGun = this.gameObject.AddComponent<RainGun>();

            SpawnBulletsEffect effect = player.gameObject.AddComponent<SpawnBulletsEffect>();
            effect.SetDirection(new Vector3(0f, -1f, 0f));
            effect.SetPosition(new Vector3(0f, 100f, 0f));
            effect.SetNumBullets(200);
            effect.SetTimeBetweenShots(0.05f);

            SpawnBulletsEffect.CopyGunStats(gun, newGun);

            newGun.damage = 0.15f;
            newGun.damageAfterDistanceMultiplier = 1f;
            newGun.reflects = 0;
            newGun.bulletDamageMultiplier = 1f;
            newGun.projectileSpeed = 1f;
            newGun.projectielSimulatonSpeed = 1f;
            newGun.projectileSize = 1f;
            newGun.projectileColor = Color.blue;
            newGun.spread = 0.75f;
            newGun.destroyBulletAfter = 100f;
            newGun.numberOfProjectiles = 1;
            newGun.ignoreWalls = false;
            newGun.damageAfterDistanceMultiplier = 1f;
            newGun.objectsToSpawn = new ObjectsToSpawn[] { PreventRecursion.stopRecursionObjectToSpawn };


            effect.SetGun(newGun);

            ColorFlash thisColorFlash = this.player.gameObject.GetOrAddComponent<ColorFlash>();
            thisColorFlash.SetNumberOfFlashes(10);
            thisColorFlash.SetDuration(0.15f);
            thisColorFlash.SetDelayBetweenFlashes(0.15f);
            thisColorFlash.SetColor(Color.blue);

            return new List<MonoBehaviour> { effect };
        }
        public List<MonoBehaviour> Effect_WallEffect(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            Gun newGun = this.gameObject.AddComponent<WallGun>();

            SpawnBulletsEffect effect = player.gameObject.AddComponent<SpawnBulletsEffect>();
            effect.SetDirection(new Vector3(0f, -1f, 0f));
            effect.SetPosition(new Vector3(0f, 100f, 0f));
            effect.SetNumBullets(100);
            effect.SetTimeBetweenShots(0.05f);

            SpawnBulletsEffect.CopyGunStats(gun, newGun);

            newGun.damage = 10f;
            newGun.damageAfterDistanceMultiplier = 1f;
            newGun.reflects = 0;
            newGun.bulletDamageMultiplier = 1f;
            newGun.projectileSpeed = 1f;
            newGun.projectielSimulatonSpeed = 1f;
            newGun.projectileSize = 1f;
            newGun.projectileColor = Color.white;
            newGun.spread = 0f;
            newGun.destroyBulletAfter = 20f;
            newGun.numberOfProjectiles = 1;
            newGun.ignoreWalls = true;
            newGun.damageAfterDistanceMultiplier = 1f;
            newGun.objectsToSpawn = new ObjectsToSpawn[] { PreventRecursion.stopRecursionObjectToSpawn };

            Traverse.Create(newGun).Field("spreadOfLastBullet").SetValue(0f);

            effect.SetGun(newGun);

            ColorFlash thisColorFlash = this.player.gameObject.GetOrAddComponent<ColorFlash>();
            thisColorFlash.SetNumberOfFlashes(10);
            thisColorFlash.SetDuration(0.15f);
            thisColorFlash.SetDelayBetweenFlashes(0.15f);
            thisColorFlash.SetColor(Color.white);

            return new List<MonoBehaviour> { effect };
        }

        [PunRPC]
        public void RPCA_SetNewEffectID(int effectID)
        {
            this.effectID = effectID;
        }
        [PunRPC]
        public void RPCA_SetNewDuration(float duration)
        {
            this.effectDuration = duration;
        }
        [PunRPC]
        public void RPCA_ApplyCurrentEffect()
        {
            this.currentEffects = this.effectFuncs[this.effectID](this.player, this.gun, this.gunAmmo, this.data, this.health, this.gravity, this.block, this.statModifiers);

            // only add a color flash if there isn't already one active
            if (this.player.gameObject.GetComponent<ColorFlash>() == null)
            {
                ColorFlash thisColorFlash = this.player.gameObject.GetOrAddComponent<ColorFlash>();
                thisColorFlash.SetNumberOfFlashes(3);
                thisColorFlash.SetDuration(0.25f);
                thisColorFlash.SetDelayBetweenFlashes(0.25f);
                thisColorFlash.SetColorMax(Color.black);
                thisColorFlash.SetColorMin(Color.black);
            }
            this.ResetEffectTimer();
        }
        [PunRPC]
        public void RPCA_ClearEffects()
        {
            if (this.currentEffects != null)
            {
                foreach (MonoBehaviour currentEffect in this.currentEffects)
                {
                    if (currentEffect != null)
                    {
                        Destroy(currentEffect);
                    }
                }
            }
            this.currentEffects = new List<MonoBehaviour>();
            if (!this.ready && this.framesWaited < this.framesToWait)
            {
                this.framesWaited++;
            }
            else if (!this.ready)
            {
                this.framesWaited = 0;
                this.ready = true;
            }

        }

    }

    public class NukeGun : Gun
    { }
    public class WallGun : Gun
    { }

    public class RainGun : Gun
    { }
}
