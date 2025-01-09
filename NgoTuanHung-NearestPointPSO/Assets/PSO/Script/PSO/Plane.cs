
using System;
using UnityEngine;

public class Plane
{
	public double a;
	public double b;
	public double c;
	public double d;
	public double[] normal;
	public Vector3 normalVector3;
	
	public Plane(double[] normal, double[] passedPoint)
	{
		this.normal = normal.Normalized();
		a = this.normal[0];
		b = this.normal[1];
		c = this.normal[2];
		d = - this.normal.Dot(passedPoint);
		normalVector3 = new Vector3((float)a, (float)b, (float)c);
	}
	
	public double CalculateDistance(Vector3 point)
	{
		return Math.Abs(a*point.x + b*point.y + c*point.z + d);
	}
	
	public double PlaneFunction(Vector3 point)
	{
		return a*point.x + b*point.y + c*point.z + d;
	}
}