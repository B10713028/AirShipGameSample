using UnityEngine;
using System.Collections;

public class BulletController : MonoBehaviour
{
    private float speed = 50f;
    private float timeToDestory = 10f;

    public Vector3 target { get; set; }
    public bool hit { get; set; }

    private void OnEnable()
    {
        //time to destory
        Destroy(gameObject, timeToDestory);
        //ignore layer
        Physics.IgnoreLayerCollision(7, 7);
    }

    void Update()
    {
        //bullet moving
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
        //destroy object when hit or travel certain distance
        if(!hit && Vector3.Distance(transform.position, target) < .01f){
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        ContactPoint contact = collision.GetContact(0);
        //damage enemy if collision
        if(collision.gameObject.TryGetComponent<Enemy>(out Enemy enemyComponent)){
            enemyComponent.TakeDamage(1);
        }
        Destroy(gameObject);
    }
}
