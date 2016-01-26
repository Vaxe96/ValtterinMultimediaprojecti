using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour {

	// kävelynopeus
	public float speed = 8f;

	// Ampumismuuttujat
	public Transform gun;
	public GameObject bullet;
	public ParticleSystem shootFX;
	public float shootInterval = 5f;
	private float shoot_t = 0f;

	// Harhailujutut
	public float idleInterval = 5f;
	private float idle_t = 0f;
	private Vector3 idleTargetPos;

	// Pelaajan etsimisjutut
	private GameObject player;
	public Vector3 dirToPlayer;
	public bool canSeePlayer;
	public float distanceToPlayer;
	private float forgetDelay;

	// Use this for initialization
	void Start () {

		idleTargetPos = transform.position;

		player = GameObject.FindGameObjectWithTag ("Player");

		if (player == null)
			Debug.LogError ("Player not found!!!!");
	
	}

	void FixedUpdate()
	{
		if (forgetDelay > 0f) {
			forgetDelay -= Time.fixedDeltaTime;
		}

		FindPlayer ();

		if (canSeePlayer == true)
			Attack ();
		else
			Idle();
	}
	void Turn(Vector3 dir)
	{
		// Kääntyy pelaajaa kohti
		dir.y = 0f;
		Quaternion newRotation = Quaternion.LookRotation (dir);
		transform.rotation = Quaternion.Lerp (transform.rotation,
		                                      newRotation,
		                                      0.3f);
	}
	void Walk()
	{
		// Kävelee eteenpäin
		GetComponent<Rigidbody> ().AddForce (transform.forward * speed);

	}
	void Attack()
	{
		// Käännytään pelaajaa kohti
		Turn (dirToPlayer);

		// Kävellään jos ollaan kaukana
		if(Vector3.Distance(transform.position, 
		                    player.transform.position) > 3f)
			Walk ();

		// Ampuminen
		DoShooting ();

	}

	void DoShooting()
	{
		// kasvatetaan ajastinta
		shoot_t += Time.fixedDeltaTime;

		// Onko aikaa kulunut tarpeeksi?
		if (shoot_t >= 1f/shootInterval)
		{
			// nollataan ajastin
			shoot_t = 0f;

			// Luodaan klooni luodista
			GameObject clone = Instantiate(bullet,
			                               gun.position,
			                               gun.rotation)as GameObject;
			// Tuhotaan klooni viiveellä
			Destroy (clone, 5f);
			
			// Lisätään vauhtia luodille
			clone.GetComponent<Rigidbody>().velocity = clone.transform.forward*40f;
			
			// Laukastaan ampumisefekti
			shootFX.Play();
		}
	}
	void Idle()
	{
		// kasvatetaan ajastinta
		idle_t += Time.fixedDeltaTime;
		
		// Onko aikaa kulunut tarpeeksi?
		if (idle_t >= idleInterval)
		{
			// Akalaskurin nollaus
			idle_t = (float) (Random.Range (0, idleInterval*10)) /10;

			// Arvotaan uusi sijainti
			idleTargetPos = transform.position;
			idleTargetPos.x += (float) (Random.Range (-100, 100)) /30;
			idleTargetPos.z += (float) (Random.Range (-100, 100)) /30;
		}

		// Lasketaan kävelysuunta
		if(Vector3.Distance(transform.position, idleTargetPos) > 1f)
		{
			Vector3 dir = idleTargetPos - transform.position;
			dir.Normalize ();

			// Kävellään
			Turn (dir);
			Walk();
		}
	}
	void FindPlayer () {

		// Lasketaan etäisyys pelaajaan
		distanceToPlayer = Vector3.Distance (player.transform.position,
		                                    transform.position);

		// Oletuksena ei nähdä pelaajaa
		if (forgetDelay <= 0f)
			canSeePlayer = false;

		// Onko tarpeeks lähellä?
		if (distanceToPlayer <= 10f || forgetDelay>0f)
		{
			// Lasketaan suunta pelaajaan
			dirToPlayer = player.transform.position - transform.position;
			dirToPlayer.Normalize ();

			// Onko pelaaja vihollisen näkökentässä?
			if (Vector3.Dot (transform.forward, dirToPlayer) >= 0.75f) {

				// Onko välissä esteitä?
				if (Physics.Raycast (transform.position, 
				                     dirToPlayer, 
				                     distanceToPlayer) == false) {

					// Nähdään pelaaja
					canSeePlayer = true;
					forgetDelay = 3f;
				}
			}
		}
	}

	void OnDrawGizmos()
	{
		Gizmos.DrawLine (transform.position, idleTargetPos);

	}

	void OnCollisionEnter(Collision col)
	{
		if (col.gameObject.tag == "Bullet") {
			canSeePlayer = true;
			forgetDelay = 5f;
		}
	}
}
