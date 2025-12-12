using System;
using UnityEngine;

public class DebugCube : MonoBehaviour
{
    [Range(0.1f, 5f), SerializeField] private float speed = 1.0f;
    [SerializeField] private float raycastOffset = 0.4f;

    private float elapsedTime = 0f;
    private float durationBeetweenTicks = 1f;
    private float ratio;
    private float cubeSide = 1f;
    private float cubeDiagonal, offsetY;
    private float raycastDistance;

    [SerializeField] private string groundTag = "Ground";
    [SerializeField] private string actionTag = "Action";


    private Action doAction;
    private RaycastHit hit;
    
    private Vector3 fromPosition, toPosition, movementDirection;
    private Quaternion fromRotation, toRotation, movementRotation;

    private void Start()
    {
        cubeDiagonal = Mathf.Sqrt(2) * cubeSide;
        offsetY = cubeDiagonal / 2 - cubeSide / 2;

        raycastDistance = cubeSide / 2 + raycastOffset;

        movementDirection = Vector3.forward;
        movementRotation = Quaternion.AngleAxis(90f, transform.right);
        
        doAction = DoActionVoid;
    }

    private void Update()
    {
        Tick();
        doAction();
    }

    private void Tick()
    {
        if(elapsedTime >= durationBeetweenTicks)
        {
            CheckCollision();
            elapsedTime = 0f;
            Debug.Log("Tick");
        }
        elapsedTime += Time.deltaTime * speed;

        ratio = elapsedTime / durationBeetweenTicks;        
    }

    private void SetModeVoid()
    {
        doAction = DoActionVoid;
    }

    private void DoActionVoid()
    {

    }

    private void SetModeMove()
    {
        fromRotation = transform.rotation;
        fromPosition = transform.position;

        toPosition = fromPosition + movementDirection;
        toRotation = movementRotation * fromRotation;

        doAction = DoActionMove;
    }

    private void DoActionMove()
    {
        transform.SetPositionAndRotation(Vector3.Lerp(fromPosition, toPosition, ratio)
            + Vector3.up * (offsetY * Mathf.Sin(Mathf.PI * ratio)), Quaternion.Lerp(fromRotation, toRotation, ratio));


    }

    private void SetDirection(Vector3 pDirection)
    {
        movementDirection = pDirection;
        movementRotation = Quaternion.AngleAxis(90f, Vector3.Cross(Vector3.up, pDirection));
    }
    
    private void SetModeFall()
    {
        fromPosition = transform.position;
        toPosition = fromPosition + Vector3.down;

        doAction = DoActionFall;
    }

    private void DoActionFall()
    {
        transform.position = Vector3.Lerp(fromPosition, toPosition, ratio);
    }

    private void CheckCollision()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out hit, raycastDistance))
        {
            GameObject lCollided = hit.collider.gameObject;
            Debug.Log("Collided = " + lCollided.name);
            Debug.DrawRay(transform.position, Vector3.down * raycastDistance, Color.green);

            if (lCollided.CompareTag(groundTag)) SetModeMove();

            if (lCollided.CompareTag(actionTag))
            {
                SetDirection(lCollided.transform.forward);
                SetModeMove();
            }

        }
        else
        {
            Debug.Log("Fall");
            SetModeFall();
        }
    }
}