using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mover : MonoBehaviour
{

    Material RecentCubeMaterial;

    Transform RecentTransform;
    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Input.GetAxis("Horizontal") * Time.deltaTime * 15f,0f, Input.GetAxis("Vertical")* Time.deltaTime * 15f, Space.Self);
        transform.Rotate(0f, Input.GetAxis("Mouse X") * Time.deltaTime * 30f, 0f, Space.World);
        transform.Rotate(-Input.GetAxis("Mouse Y") * Time.deltaTime * 30f, 0f,0f, Space.Self);

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            GameObject obj = (GameObject)Instantiate(Resources.Load("Cube"));
            obj.transform.position = this.transform.position + transform.forward*6f;
            obj.GetComponent<OctreeItem>().RefreshOwner();
        }


        RaycastHit hit;

        if (Physics.Raycast(transform.position, transform.forward, out hit, 100f))
        {
            if (hit.collider.CompareTag("OctCube"))
            {
                if (RecentCubeMaterial != null)
                {
                    RecentCubeMaterial.color = Color.white;
                }
                GameObject caught = hit.collider.gameObject;
                Rigidbody rigid = caught.GetComponent<Rigidbody>();
                RecentCubeMaterial = caught.GetComponent<Renderer>().material;

                RecentCubeMaterial.color = Color.cyan;

                if (Input.GetKeyDown(KeyCode.Mouse1))
                {
                    rigid.isKinematic = true;
                    RecentTransform = caught.transform;

                    RecentTransform.parent = transform;
                }
                if (Input.GetKeyUp(KeyCode.Mouse1))
                {
                    rigid.isKinematic = false;
                    if (RecentTransform != null)
                    {
                        RecentTransform.parent = null;
                    }
                }
                if (Input.GetKeyUp(KeyCode.E))
                {
                    Destroy(caught.gameObject);
                }
                if (Input.GetKeyDown(KeyCode.R))
                {
                  
                    rigid.AddForce(transform.forward * 150f);
                }
            }
        }
        else
        {
            if (RecentCubeMaterial != null)
            {
                RecentCubeMaterial.color = Color.white;
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }
}
