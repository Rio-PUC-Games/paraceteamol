﻿using UnityEngine;
using System.Collections;

public class BallPhysics : MonoBehaviour
{
	[FMODUnity.EventRef]
	public string inputsound;
	[Header("Physics")]
	[Tooltip("Velocidade da bola")]
	public float Speed;
	[Tooltip("Direcao inicial da bola")]
	public Vector2 Direction = Vector2.one;
	private GameObject GameManager;
	private MatchScript _matchScript;
	private GameObject playerCurrentlyHolding;

	private AudioSource BallSound;
	private Rigidbody2D _rb;
	private Collider2D _col;
	private float _startingSpeed;
	private Collider2D _tempCol;


	#region StateMachine
	public enum State
	{
		Idle,
		Held,
		Release,
	}
	public State state;

	IEnumerator IdleState()
	{
		while (state == State.Idle)
			yield return 0;
	}

	IEnumerator HeldState()
	{
		while (state == State.Held)
			yield return 0;
	}

	IEnumerator ReleaseState()
	{
		while (state == State.Release)
			yield return 0;
	}
	#endregion
	/*
	 * Idle
	 * Held
	 * Release
	 */

	private void Awake()
	{
		GameManager = GameObject.Find("GameManeger");
		_rb = GetComponent<Rigidbody2D>();
		_rb.AddForce(Direction * Speed, ForceMode2D.Impulse);
		BallSound = GetComponent<AudioSource>();
		_col = gameObject.GetComponent<Collider2D>();
		_startingSpeed = Speed;
	}

	private void FixedUpdate()
	{
		Direction = _rb.velocity;

		switch (state)
		{
			case State.Held:
				_rb.velocity = Vector2.zero;
				break;
			case State.Release:
				Speed = _startingSpeed;
				_rb.velocity = -ReleaseDirection(playerCurrentlyHolding.transform.position) * Speed;
				state = State.Idle;
				break;
		}
	}

	public void SetPlayerCurrentlyHolding(GameObject player)
	{
		playerCurrentlyHolding = player;
	}

	private void OnCollisionEnter2D(Collision2D col)
	{
		switch (state)
		{
			case State.Idle:
				if (col.gameObject.CompareTag("Ground") || col.gameObject.CompareTag("Wall") || col.gameObject.CompareTag("Player") || col.gameObject.CompareTag("MovingPlatform"))
				{
					if (col.gameObject.CompareTag("Player"))
					{
						// Bota aqui pra tocar o som.
					}

					ReflectProjectile(_rb, col.contacts[0].normal);
				}
				break;
			case State.Held:
				if (col.gameObject.CompareTag("Player") || col.gameObject.CompareTag("Wall") || col.gameObject.CompareTag("MovingPlatform"))
					Physics2D.IgnoreCollision(col.gameObject.GetComponent<Collider2D>(), gameObject.GetComponent<Collider2D>(), true);
				break;
			case State.Release:
				Physics2D.IgnoreCollision(col.gameObject.GetComponent<Collider2D>(), gameObject.GetComponent<Collider2D>(), false);
				break;
		}
	}

	private void OnTriggerEnter2D(Collider2D col)
	{
		if (col.gameObject == GameObject.Find("Time1"))
		{
			GameManager.GetComponent<MatchScript>().RedTeamGol();
		}
		else if (col.gameObject == GameObject.Find("Time2"))
		{
			GameManager.GetComponent<MatchScript>().BlueTeamGol();
		}
	}

	private void ReflectProjectile(Rigidbody2D rb, Vector2 reflectVector)
	{
		Direction = Vector2.Reflect(Direction, reflectVector);
		_rb.velocity = Speed * Direction.normalized;
		FMODUnity.RuntimeManager.PlayOneShot(inputsound);
	}

	private Vector2 ReleaseDirection(Vector2 playerPos)
	{
		Vector2 dir = gameObject.transform.position;
		dir = playerPos - dir;
		return dir.normalized;
	}
}