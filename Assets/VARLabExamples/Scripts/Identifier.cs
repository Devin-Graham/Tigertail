using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TigerTail
{
    
    public class Identifier : MonoBehaviour
    {
        public static string selectedObject;
        public string internalObject;
        public float rayLength = 100;
        public LayerMask layermask;
        public float appliedForce = 15000f;

        public Transform holdParent;

        private GameObject heldObject;

        public float moveForce = 250;

        private void Update()
        {

            if (Input.GetKeyDown(KeyCode.E))
            {
                if(heldObject == null)
                {
                    RaycastHit hit;

                    if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, rayLength))
                    {
                        var selection = hit.transform;

                        PickupObject(selection.gameObject);
                    }
                }
                else
                {
                    DropObject();
                }
               
            }
            else if (Input.GetMouseButtonDown(0))
            {
                if(heldObject != null)
                {
                    ThrowObject();
                }
            }

            if(heldObject != null)
            {
                MoveObject();
            }
            
        }

        void MoveObject()
        {
            if(Vector3.Distance(heldObject.transform.position, holdParent.position) > 0.1f)
            {
                Vector3 moveDirection = (holdParent.position - heldObject.transform.position);
                heldObject.GetComponent<Rigidbody>().AddForce(moveDirection * moveForce);
            }
        }

        void PickupObject(GameObject pickObj)
        {
            if(pickObj.GetComponent<Rigidbody>())
            {
                Rigidbody objRig = pickObj.GetComponent<Rigidbody>();
                objRig.useGravity = false;
                objRig.drag = 10;

                objRig.transform.parent = holdParent;
                heldObject = pickObj;
            }
        }

        void DropObject()
        {
            Rigidbody heldRig = heldObject.GetComponent<Rigidbody>();

            heldRig.useGravity = true;
            heldRig.drag = 1;

            heldObject.transform.parent = null;
            heldObject = null;
            
        }

        public void ThrowObject()
        {
            Rigidbody heldRig = heldObject.GetComponent<Rigidbody>();

            heldRig.useGravity = true;
            heldRig.drag = 1;

            heldObject.transform.parent = null;
            heldObject = null;
            heldRig.AddForce(Camera.main.transform.forward * appliedForce);
        }

    }
}

