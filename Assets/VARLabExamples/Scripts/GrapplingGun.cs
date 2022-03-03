using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TigerTail
{
    public class GrapplingGun : MonoBehaviour
    {
        private LineRenderer lr;
        private Vector3 grapplePoint;
        public LayerMask whatIsGrappleable;
        public Transform gunTip, playerCamera, player;
        private float maxDistance = 20f;
        private SpringJoint joint;
        public float grappleForce = 0.00005f;
        private Vector3 grappleDir;

        private bool isGrappling;

        private Rigidbody grappleObject;

        public Rigidbody projectile;

        private void Awake()
        {
            lr = GetComponent<LineRenderer>();
        }

        private void Update()
        {

            if (Input.GetMouseButtonDown(0))
            {
                StartGrapple();
            }
            else if (Input.GetMouseButtonUp(0) && isGrappling == true)
            {
                StopGrapple();
                if (grappleObject != null && grappleObject.gameObject.CompareTag("Moveable"))
                {
                    PullObject();
                }
                isGrappling = false;
            }
            else if (Input.GetMouseButtonDown(1))
            {
                ShootSnowball();
            }



        }

        private void LateUpdate()
        {
            DrawRope();
        }

        void StartGrapple()
        {
            RaycastHit hit;
            if (Physics.Raycast(playerCamera.position, playerCamera.forward, out hit, maxDistance, whatIsGrappleable))
            {
                grapplePoint = hit.point;
                joint = player.gameObject.AddComponent<SpringJoint>();
                joint.autoConfigureConnectedAnchor = false;
                joint.connectedAnchor = grapplePoint;

                float distanceFromPoint = Vector3.Distance(player.position, grapplePoint);

                joint.maxDistance = distanceFromPoint * 0.8f;
                joint.minDistance = distanceFromPoint * 0.25f;

                joint.spring = 4.5f;
                joint.damper = 7f;
                joint.massScale = 2.5f;

                lr.positionCount = 2;

                Rigidbody playerRig = player.GetComponent<Rigidbody>();

                isGrappling = true;

                player.GetComponent<Rigidbody>().AddForce(Camera.main.transform.forward * 800.0f);

                if (hit.transform.gameObject.GetComponent<Rigidbody>() != null)
                {
                    grappleObject = hit.transform.gameObject.GetComponent<Rigidbody>();
                }



            }
        }

        void DrawRope()
        {
            if (!joint) return;
            lr.SetPosition(0, gunTip.position);
            lr.SetPosition(1, grapplePoint);
        }

        void StopGrapple()
        {
            lr.positionCount = 0;
            Destroy(joint);
            player.GetComponent<Rigidbody>().AddForce(Camera.main.transform.forward * 1000.0f);

        }

        void PullObject()
        {
            grappleObject.AddForce(-(Camera.main.transform.forward * 300f * Vector3.Distance(player.position, grapplePoint)));
            grappleObject = null;
        }

        void ShootSnowball()
        {
            Rigidbody snowballInstance;
            snowballInstance = Instantiate(projectile, gunTip.position, gunTip.rotation) as Rigidbody;
            snowballInstance.AddForce(Camera.main.transform.forward * 4000f);

            snowballInstance.GetComponent<ThrowableSnowball>().state = ThrowableSnowball.State.Thrown;
        }
    }
}


