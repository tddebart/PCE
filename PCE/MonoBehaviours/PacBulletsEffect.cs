﻿using UnityEngine;
using UnboundLib;
using Photon.Pun;
using PCE.Extensions;

namespace PCE.MonoBehaviours
{
    public class PacBulletsAssets
    {
        private static GameObject _pacbullet = null;

        internal static GameObject pacbullet
        {
            get
            {
                if (PacBulletsAssets._pacbullet != null) { return PacBulletsAssets._pacbullet; }
                else
                {
                    PacBulletsAssets._pacbullet = new GameObject("PacBullet", typeof(PacBulletEffect), typeof(PhotonView));
                    UnityEngine.GameObject.DontDestroyOnLoad(PacBulletsAssets._pacbullet);

                    return PacBulletsAssets._pacbullet;
                }
            }
            set { }
        }
    }
    public class PacBulletSpawner : MonoBehaviour
    {
        private static bool Initialized = false;



        void Awake()
        {
            if (!Initialized)
            {
                PhotonNetwork.PrefabPool.RegisterPrefab(PacBulletsAssets.pacbullet.name, PacBulletsAssets.pacbullet);
            }
        }

        void Start()
        {
            if (!Initialized)
            {
                Initialized = true;
                return;
            }

            if (!PhotonNetwork.OfflineMode && !this.gameObject.transform.parent.GetComponent<ProjectileHit>().ownPlayer.data.view.IsMine) return;


            PhotonNetwork.Instantiate(
                PacBulletsAssets.pacbullet.name,
                transform.position,
                transform.rotation,
                0,
                new object[] { this.gameObject.transform.parent.GetComponent<PhotonView>().ViewID }
            );
        }
    }
    [RequireComponent(typeof(PhotonView))]
    public class PacBulletEffect : MonoBehaviour, IPunInstantiateMagicCallback
    {
        private int numWraps = 0;
        private int wraps = 0;
        private PhotonView view;
        private Transform parent;
        private Player player;
        private Gun gun;
        private ProjectileHit projectile;
        private Camera mainCam;

        public void OnPhotonInstantiate(Photon.Pun.PhotonMessageInfo info)
        {
            object[] instantiationData = info.photonView.InstantiationData;

            GameObject parent = PhotonView.Find((int)instantiationData[0]).gameObject;

            this.gameObject.transform.SetParent(parent.transform);

            this.player = parent.GetComponent<ProjectileHit>().ownPlayer;
            this.gun = this.player.GetComponent<Holding>().holdable.GetComponent<Gun>();
        }

        void Awake()
        {

        }
        void Start()
        {
            this.parent = this.gameObject.transform.parent;
            if (this.parent == null) { return; }
            this.projectile = this.gameObject.transform.parent.GetComponent<ProjectileHit>();
            this.view = this.gameObject.GetComponent<PhotonView>();
            this.numWraps = this.gun.GetAdditionalData().wraps;

            this.mainCam = MainCam.instance.transform.GetComponent<Camera>();
        }
        void Update()
        {
            if (this.parent == null) { return; }

            if (this.wraps >= this.numWraps)
            {
                Destroy(this);
                return;
            }


            Vector3 pos = this.mainCam.WorldToScreenPoint(this.transform.position);
            pos.x /= (float)Screen.width;
            pos.y /= (float)Screen.height;

            bool flag = false;

            if (pos.x > 1f || pos.x < 0f)
            {
                flag = true;
                pos.x = 1f - pos.x;
            }
            if (pos.y > 1f || pos.y < 0f)
            {
                flag = true;
                pos.y = 1f - pos.y;
            }
            if (flag)
            {
                // offline
                if (PhotonNetwork.OfflineMode)
                {
                    this.RPCA_WrapBullet(pos.x, pos.y, pos.z);
                }
                // network
                else if (this.view.IsMine)
                {
                    this.view.RPC("RPCA_WrapBullet", RpcTarget.All, new object[] {pos.x, pos.y, pos.z});
                }
            }
        }
        [PunRPC]
        private void RPCA_WrapBullet(float x, float y, float z)
        {
            this.projectile.GetAdditionalData().startTime = Time.time;
            this.projectile.GetAdditionalData().inactiveDelay = float.MaxValue;

            TrailRenderer[] trailRenderers = this.projectile.GetComponentsInChildren<TrailRenderer>();
            Renderer[] renderers = this.projectile.GetComponentsInChildren<Renderer>();
            ParticleSystem[] particleSystems = this.projectile.GetComponentsInChildren<ParticleSystem>();

            foreach (ParticleSystem particleSystem in particleSystems)
            {
                particleSystem.Clear(true);
            }
            foreach (TrailRenderer trailRenderer in trailRenderers)
            {
                trailRenderer.Clear();
            }
            foreach (Renderer renderer in renderers)
            {
                renderer.enabled = false;
            }

            this.parent.transform.position = this.mainCam.ScreenToWorldPoint(new Vector3(x * (float)Screen.width, y * (float)Screen.height, z));
            this.wraps++;

            this.ExecuteAfterFrames(2, () =>
            {
                foreach (Renderer renderer in renderers)
                {
                    renderer.enabled = true;
                }
                foreach (TrailRenderer trailRenderer in trailRenderers)
                {
                    trailRenderer.Clear();
                }
                foreach (ParticleSystem particleSystem in particleSystems)
                {
                    particleSystem.Clear(true);
                }
            });
            this.ExecuteAfterFrames(5, () => this.projectile.GetAdditionalData().inactiveDelay = 0f);
        }
    }
}
