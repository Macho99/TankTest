using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public interface IHittable
{
	public Int64 HitID { get; }
	public void ApplyDamage(Transform source, Vector3 point, Vector3 force, int damage);
}