using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Troop : MonoBehaviour
{
    public enum TroopType { Small, Medium, Large }
    public TroopType troopType;
    public int health;
    public int speed;

    private bool isMoving = false;
    private Vector3 targetPosition;

    void Start()
    {
        targetPosition = transform.position;
    }

    public void MoveForward()
    {
        if (!isMoving)
        {
            int direction = (transform.rotation.eulerAngles.y == 0) ? 1 : -1;
            targetPosition += new Vector3(0, 0, direction * 1);
            StartCoroutine(MoveToTarget());
        }
    }

    IEnumerator MoveToTarget()
    {
        isMoving = true;
        float time = 0;
        Vector3 startPosition = transform.position;

        while (time < 1f / speed)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, time * speed);
            time += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
        isMoving = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        Troop otherTroop = other.GetComponent<Troop>();
        if (otherTroop)
        {
            ResolveCollision(otherTroop);
        }
    }

    void ResolveCollision(Troop other)
    {
        if (other.troopType == troopType)
        {
            StopMovement();
        }
        else if (troopType == TroopType.Large && other.troopType == TroopType.Small)
        {
            Destroy(other.gameObject);
        }
        else if (troopType == TroopType.Small && other.troopType == TroopType.Large)
        {
            Destroy(gameObject);
        }
    }

    void StopMovement()
    {
        isMoving = true; // Se detiene hasta que una tropa muera.
    }
}