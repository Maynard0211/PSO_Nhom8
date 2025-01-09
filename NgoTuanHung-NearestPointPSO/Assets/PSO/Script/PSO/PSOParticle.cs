using System;
using UnityEngine;

public class PSOParticle
{
	public int dimension;
	public double[] position;
	public double[] velocity;
	public double[] pbest;
	public double fbest;
	public Func<double[], double> func;
	public GameObject particleGameObject;

	public PSOParticle(int dimension, double[] position, double[] velocity, Func<double[], double> func, GameObject particleGameObject)
	{
		this.dimension = dimension;
		this.position = new double[dimension];
		this.velocity = new double[dimension];
		this.func = func;
		
		for (int i=0;i<dimension;i++)
		{
			this.position[i] = position[i];
			this.velocity[i] = velocity[i];
		}
		
		this.velocity = this.velocity.Normalized();
		pbest = (double[])this.position.Clone();
		fbest = this.func(pbest);
		
		this.particleGameObject = particleGameObject;
		particleGameObject.transform.position = this.position.ToVector3();
		particleGameObject.transform.rotation = Quaternion.LookRotation(this.velocity.ToVector3(), Vector3.up);
	}
	
	public double SelfFunc() => func(position);
	
	public PSOParticle()
	{
		
	}
}