using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DrawerMech : MonoBehaviour 
{

	public Vector3 OpenPosition, ClosePosition;

	float moveSpeed;

    float lerpTimer;

    public bool drawerBool;

    private bool playerInTrigger = false;

	void Start()
	{
        drawerBool = false;
	}

	void OnTriggerEnter(Collider col)
	{
		if(col.gameObject.tag == ("Player"))
		{
			playerInTrigger = true;
		}
	}

	void OnTriggerExit(Collider col)
	{
		if(col.gameObject.tag == ("Player"))
		{
			playerInTrigger = false;
		}
	}

	void Update()
	{
		// Check for E key press using new Input System
		if (playerInTrigger && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
		{
			drawerBool = !drawerBool;
		}

        if (drawerBool)
        {
            moveSpeed = +1f;

            lerpTimer = Mathf.Clamp(lerpTimer + Time.deltaTime * moveSpeed, 0f, 1f);

            transform.localPosition = Vector3.Lerp(ClosePosition, OpenPosition, lerpTimer);
        }
            
        else
        {
            moveSpeed = -1f;

            lerpTimer = Mathf.Clamp(lerpTimer + Time.deltaTime * moveSpeed, 0f, 1f);

            transform.localPosition = Vector3.Lerp(ClosePosition, OpenPosition, lerpTimer);
        }

    }

}
