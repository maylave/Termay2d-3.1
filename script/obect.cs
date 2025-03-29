using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class obect : MonoBehaviour
{
    public float distance = 1.0f;
    public float radius = 0.5f;
    public LayerMask layerMask;
    public string targetObjectName = "Block";
    public GameObject objectToCreate;

    private int createdObjectsCount = 0;
   public bool isObjectOnLeft = false;
    public  bool isObjectOnRight = false;
    public bool isObjectBelow = false;

    private void FixedUpdate()
    {
        //if (isObjectOnRight == false)
        //{ 

        //    Vector2 rayDirection = Vector2.right; // направление луча (вправо)
        //float rayDistance = 1f; // дистанция по которой луч должен броситься

        //// создаем луч
        //RaycastHit2D hit = Physics2D.Raycast(transform.position, rayDirection, rayDistance);

        //// проверяем, есть ли объект справа
        
        
        //        if(hit.collider.CompareTag("water") || hit.collider.CompareTag("Grag"))
        //        {
        //            Debug.Log("Блок справа найден!");
        //            isObjectOnRight = true;
        //        }
        //    else 
        //    {
        //        Instantiate(objectToCreate, transform.position + new Vector3(rayDistance, 0, 0), Quaternion.identity);
        //        Debug.Log("Блок справа не найден!");
        //    }



        //}
        // Checking left
        //if (!isObjectOnLeft /*&& createdObjectsCount < 2*/)
        //{
        //    RaycastHit2D hitLeft = Physics2D.Raycast(transform.position, Vector2.left, distance);
           
        //    //isObjectOnLeft = hitLeft.collider != null && hitLeft.collider.gameObject.name == w;
        //    if (!hitLeft /*.transform.tag != ("water") *//*|| hitLeft.transform.tag != ("Grag")*/)
        //    {
        //        Instantiate(objectToCreate,   new Vector2(transform.position.x - distance, transform.position.y), Quaternion.identity);
        //        createdObjectsCount++; isObjectOnLeft = true;
        //    }
        //    else
        //    {
        //        isObjectOnLeft = true;
        //    }

        //}

        //// Checking right
        //if (!isObjectOnRight && createdObjectsCount < 2)
        //{
        //    RaycastHit2D hitRight = Physics2D.Raycast(transform.position, Vector2.right, distance, layerMask);
        //    isObjectOnRight = hitRight.collider != null && hitRight.collider.gameObject.name == targetObjectName;
        //    if (!isObjectOnRight)
        //    {
        //        Instantiate(objectToCreate, transform.position + new Vector3(distance, 0, 0), Quaternion.identity);
        //        createdObjectsCount++; isObjectOnRight = true;
        //    }
        //}

        //// Checking down
        //if (!isObjectBelow && createdObjectsCount < 2)
        //{
        //    Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position - new Vector3(0, distance, 0), radius, layerMask);
        //    isObjectBelow = false;
        //    for (int i = 0; i < colliders.Length; i++)
        //    {
        //        if (colliders[i].gameObject.name == targetObjectName)
        //        {
        //            isObjectBelow = true;
        //            break;
        //        }
        //    }
        //    if (!isObjectBelow)
        //    {
        //        Instantiate(objectToCreate, transform.position - new Vector3(0, distance, 0), Quaternion.identity);
        //        createdObjectsCount++; isObjectBelow = true;
        //    }
        //}

        //// Disabling script if object exists
        //if (isObjectOnLeft || isObjectOnRight || isObjectBelow)
        //{
        //    enabled = false;
        //}
    }
}
