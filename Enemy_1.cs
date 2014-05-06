using UnityEngine;
using System.Collections;

public class Enemy_1 : Enemy {

	// Use this for initialization
	protected override void Start()
	{
		base.Start();
	}
	
	// Update is called once per frame
	protected override void Update () 
	{
		base.Update();

		Vector3 pos = GetPosition();

		if (m_Target != null && !m_IsDestroyed)
		{
			Vector3 targetPos = m_Target.GetPosition();
			Vector3 dist = targetPos - pos;

			if (dist.magnitude < m_AttackRange)
			{
				Vector3 toTarget = m_Target.gameObject.transform.position - gameObject.transform.position;
				toTarget.y = 0.0f;

				if (CheckLineOfSight() && !m_IsAttacking)
				{
					StartShooting();
				}
			}
			else
			{
				if (m_IsAttacking)
				{
					StopShooting();
				}
			}
		}

		if (m_IsAttacking)
		{
			transform.LookAt(m_Target.GetPosition());

			m_CurAttackTime += Time.deltaTime;

			if (m_CurAttackTime > m_AttackSpeed)
			{
				if (m_Target != null && m_Target.GetHealth() > 0 && CheckLineOfSight())
				{
					SpawnProjectile(m_Target);
					m_CurAttackTime -= m_AttackSpeed;
				}
				else
				{
					StopShooting();
				}
			}
		}
	}

	private Projectile SpawnProjectile(Entity a_Target)
	{
		Vector3 spawnPos = GetPosition();
		spawnPos.y += 0.75f;

		Vector3 targetPos = a_Target.GetPosition();
		Vector3 attackDir = targetPos - spawnPos;
		attackDir.Normalize();

		spawnPos += attackDir * 0.45f;

		Transform t = MonoBehaviour.Instantiate(ProjectilePrefab, spawnPos, Quaternion.identity) as Transform;
		t.name = "Projectile";
		Projectile proj = t.GetComponent<Projectile>();
		proj.RetrieveColors();
		proj.SetOwner(this);
		proj.SetTarget(a_Target);
		proj.SetAttackDirection(attackDir);
		proj.SetHitDamage(m_AttackDamage);

		if (m_ShootSound != null)
		{
			m_ShootSound.Play();
		}

		PlayNote( proj );

		return proj;
	}

	protected override void SetProjectileColor(Projectile proj, MidiResponse melody)
	{
		ProjectileEnemy1 proj1 = (ProjectileEnemy1)proj;
		proj1.SetColor((int)melody.mNotes[m_NextNoteBullet]);
	}
}
