﻿using UnityEngine;
using System.Collections;

public enum Directions {
	up, down, left, right
}

public class Zoomer : MonoBehaviour {

	private PE_Obj peo;

	private Enemy enemy;

	public float DirChangeForStuck = 0.01f;

	private Vector3 stuck_position;

	public int frames_stopped = 0;

	public int StuckFramesForSwitch = 5;

	private bool established = false;

	public Directions GroundDirection;

	public float GroundAcceleration = 10f;

	public float Velocity = .5f;

	public float DirectionChangeVelocity = 1f;

	public float hitDamage = 8f;

	private bool startMovement = false;


	private Vector3 initial_position;
	private Directions initial_direciton;

	// Use this for initialization
	void Start () {
		peo = this.GetComponent<PE_Obj>();
		enemy = this.GetComponent<Enemy>();

		initial_position = this.transform.position;
		initial_direciton = GroundDirection;

		UpdateSpriteRotation();
	}

	void UpdateSpriteRotation() {
		switch (GroundDirection) {
		case Directions.down:
			//transform.Rotate(new Vector3(0f, 0f, 0f));
			transform.rotation = Quaternion.AngleAxis(0f, Vector3.back);
			break;
		case Directions.right:
			//transform.Rotate(new Vector3(0f, 0f, 90f));
			transform.rotation = Quaternion.AngleAxis(270f, Vector3.back);
			break;
		case Directions.up:
			//transform.Rotate(new Vector3(0f, 0f, 180f));
			transform.rotation = Quaternion.AngleAxis(180f, Vector3.back);
			break;
		case Directions.left:
			//transform.Rotate(new Vector3(0f, 0f, 270f));
			transform.rotation = Quaternion.AngleAxis(90f, Vector3.back);
			break;
		}
	}

	void OnTriggerEnter(Collider other) {
		// Ignore bullet collisions, as they are handled by Enemy
		if (other.gameObject.layer == LayerMask.NameToLayer ("Player")) {
			other.GetComponent<SamusMovement>().CauseDamage(hitDamage);
		}
	}

	void OnBecameVisible(){
		startMovement = true;
	}

	public int counter = 0;
	public int num_changes = 0;

	// Update is called once per frame
	void FixedUpdate () {
		//Debug.Log(peo.vel + "," + peo.acc + "," + GroundDirection + "," + peo.velRel);
		//Debug.Log(peo.velRel.x==0);
		//Debug.Log (peo.velRel.y==0);
		if(!startMovement || enemy.frozen) return;

		bool direction_changed = false;

		if (counter++ >= 50) {
			counter = 0;
			num_changes = 0;
		}

		if (stuck_position != null) {
			if ((transform.position - stuck_position).magnitude > DirChangeForStuck) {
				frames_stopped = 0;
			} else {
				frames_stopped += 1;
				//Debug.Log("STUCK! " + transform.position + " " + stuck_position);
			}
		}

		if (frames_stopped == 0 || stuck_position == null) {
			stuck_position = transform.position;
		}

		switch (GroundDirection) {
		case Directions.up:
			peo.acc = new Vector3(0f, GroundAcceleration, 0f);
			peo.vel.x = Velocity;

			if (peo.vel.y < DirectionChangeVelocity) {
				established = true;
			}

			if ((established && peo.vel.y > DirectionChangeVelocity) || frames_stopped>StuckFramesForSwitch) {

				if ((established && peo.vel.y > DirectionChangeVelocity)) {
					GroundDirection = Directions.left;
				} else {
					GroundDirection = Directions.right;
				}
				direction_changed = true;
			}
			break;

		case Directions.left:
			peo.acc = new Vector3(-1*GroundAcceleration, 0f, 0f);
			peo.vel.y = Velocity;

			if (peo.vel.x > -1 * DirectionChangeVelocity) {
				established = true;
			}

			if ((established && peo.vel.x < -1 * DirectionChangeVelocity) || frames_stopped>StuckFramesForSwitch) {
				if (established && peo.vel.x < -1 * DirectionChangeVelocity) {
					GroundDirection = Directions.down;
				} else {
					GroundDirection = Directions.up;
				}

				direction_changed = true;
			}
			break;

		case Directions.down:
			peo.acc = new Vector3(0f, -1*GroundAcceleration, 0f);
			peo.vel.x = -1 * Velocity;

			if (peo.vel.y > -1 * DirectionChangeVelocity) {
				established = true;
			}

			if ((established && peo.vel.y < -1 * DirectionChangeVelocity) || frames_stopped>StuckFramesForSwitch) {
				if ((established && peo.vel.y < -1 * DirectionChangeVelocity)) {
					GroundDirection = Directions.right;
				} else {
					GroundDirection = Directions.left;
				}
				direction_changed = true;
			}
			
			break;

		case Directions.right:
			peo.acc = new Vector3(GroundAcceleration, 0f, 0f);
			peo.vel.y = -1 * Velocity;

			if (peo.vel.x > DirectionChangeVelocity) {
				established = true;
			}

			if ((established && peo.vel.x > DirectionChangeVelocity) || frames_stopped>StuckFramesForSwitch) {
				if ((established && peo.vel.x > DirectionChangeVelocity)) {
					GroundDirection = Directions.up;
				} else {
					GroundDirection = Directions.down;
				}
				direction_changed = true;
			}
			break;
		}

		if (direction_changed) {
			num_changes += 1;

			established = false;
			peo.vel = Vector3.zero;
			frames_stopped = 0;
			peo.acc = Vector3.zero;
			UpdateSpriteRotation();
		}

		if (num_changes >= 7) {
			Debug.Log ("stuck");

			PhysEngine.objs.Remove(peo);
			Destroy(this.gameObject);
		}
	}
}
