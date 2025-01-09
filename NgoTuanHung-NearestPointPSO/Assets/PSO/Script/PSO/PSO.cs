using System;
using System.Collections;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class PSO : MonoBehaviour 
{
	public int numParticle = 1000;
	public const int dimension = 3;
	public int iteration = 100;
	public double[] gbest; public double fgbest = double.MaxValue;
	public double w = 0.8f, c1 = 0.1f, r1 = 0.5f, c2 = 0.1f, r2 = 0.6f;
	public bool useCrossedPoint = false;
	public double[][] positionBound = new double[dimension][];
	
	public bool useCustomPositionBounds = true;
	public List<positionBound> customPositionBounds = new List<positionBound>();
	public double[] potentialCrossedPoint = new double[dimension];
	
	/* Plane info part
	- Square distance function: a²x² + b²y² + c²z² + 2acxz + 2bcyz + 2abxy + 2adx + 2bdy + 2cdz + d²
	 */
	
	public List<GameObject> planeObjects = new List<GameObject>();
	public List<Plane> planes;
	public double asqr_sum, bsqr_sum, csqr_sum, two_ac_sum, two_bc_sum, two_ab_sum,
	two_ad_sum, two_bd_sum, two_cd_sum, dsqr_sum;
	public void InitPlane()
	{
		planes = new List<Plane>();
		Vector3 forwardDir;
		planeObjects.ForEach(planeObject => 
		{
			forwardDir = planeObject.transform.forward;
			planes.Add(new Plane
			(
				new double[] {forwardDir.x, forwardDir.y, forwardDir.z},
				new double[] {planeObject.transform.position.x, planeObject.transform.position.y, planeObject.transform.position.z}
			));
		});
	}
	
	public void InitArguments()
	{
		planes.ForEach(plane => 
		{
			asqr_sum += plane.a * plane.a;
			bsqr_sum += plane.b * plane.b;
			csqr_sum += plane.c * plane.c;
			two_ac_sum += 2*plane.a*plane.c;
			two_bc_sum += 2*plane.b*plane.c;
			two_ab_sum += 2*plane.a*plane.b;
			two_ad_sum += 2*plane.a*plane.d;
			two_bd_sum += 2*plane.b*plane.d;
			two_cd_sum += 2*plane.c*plane.d;
			dsqr_sum += plane.d * plane.d;
		});
	}
	

	public double Function(double[] p)
	{
		return asqr_sum * p[0] * p[0] +
			bsqr_sum * p[1] * p[1] +
			csqr_sum * p[2] * p[2] +
			two_ac_sum * p[0] * p[2] +
			two_bc_sum * p[1] * p[2] +
			two_ab_sum * p[0] * p[1] +
			two_ad_sum * p[0] +
			two_bd_sum * p[1] +
			two_cd_sum * p[2] +
			dsqr_sum;
	}
	
	public double epsilon = 0.001;
	public List<GameObject> distanceMeasures = new List<GameObject>();
	public bool visualizeDistanceToPlaneForever = false;
	public IEnumerator VisualDistanceToPlane(bool forever)
	{
		float distanceSum;
		float distanceLength;
		double planeFunction;
		
		for (int i=0;i<planes.Count;i++)
		{
			GameObject distance = Instantiate(Resources.Load("DistanceMeasure1")) as GameObject;
			distanceMeasures.Add(distance);
		}
		
		do
		{
			distanceSum = 0;
			for (int i=0;i<planes.Count;i++)
			{
				distanceLength = (float)planes[i].CalculateDistance(transform.position);
				distanceSum += distanceLength;
				distanceMeasures[i].transform.position = transform.position;
				distanceMeasures[i].transform.localScale = new Vector3(1, 1, distanceLength);
				
				planeFunction = planes[i].PlaneFunction
				(
					transform.position + Math.Abs(Vector3.Dot
					(
						transform.position - planeObjects[i].transform.position, planes[i].normalVector3
					)) * planes[i].normalVector3
				);
				if (planeFunction < -epsilon || planeFunction > epsilon) distanceMeasures[i].transform.rotation = Quaternion.LookRotation(-planes[i].normalVector3, Vector3.up);
				else distanceMeasures[i].transform.rotation = Quaternion.LookRotation(planes[i].normalVector3, Vector3.up);
			}
			
			print($"<color=#00FF00>Distance sum: {distanceSum}</color>");
			
			yield return new WaitForSeconds(Time.fixedDeltaTime);
		} while (forever);
	}
	
	public CombinationFinder<Plane> combinationFinder;

	private void Start() 
	{
		InitPlane();
		InitArguments();
		if (useCrossedPoint) FindCrossedPoint();
		StartCoroutine(StartOptimization());	
	}
	
	public void FindCrossedPoint()
	{
		combinationFinder = new CombinationFinder<Plane>(planes, 3);
		Matrix<double> A = Matrix<double>.Build.Dense(3, 3);
		Matrix<double> B = Matrix<double>.Build.Dense(3, 1);
		GameObject crossedPoint = Instantiate(Resources.Load("CrossedPoint")) as GameObject;
		
		int[] combination = combinationFinder.FindCombinationWithCondition((int[] planeIndexes) => 
		{
			for (int i=0;i<3;i++)
			{
				A[i, 0] = planes[planeIndexes[i]].a;
				A[i, 1] = planes[planeIndexes[i]].b;
				A[i, 2] = planes[planeIndexes[i]].c;
				B[i, 0] = -planes[planeIndexes[i]].d;
			}
			
			if (Math.Abs(A.Determinant()) < epsilon) return false;
			else return true;
		});
		
		if (combination != null)
		{
			Matrix<double> X = A.Inverse().Multiply(B);
			crossedPoint.transform.position = new Vector3((float)X[0, 0], (float)X[1, 0], (float)X[2, 0]);
			for (int i=0;i<3;i++) potentialCrossedPoint[i] = X[i, 0];
		}
	}
	
	public float timeDescale = 1;
	public float weightDecay = 0.01f;
	public float weightClamp = 0.1f;
	
	public IEnumerator StartOptimization()
	{
		Init();
		PSOParticle pSOParticle; double selfFunc;
		double[] rawVelocity;
		Vector3 velocity;
		
		for (int i=0;i<iteration;i++)
		{
			for (int j=0;j<numParticle;j++)
			{
				pSOParticle = particles[j];
				
				pSOParticle.position = pSOParticle.position.Add
				(
					rawVelocity = pSOParticle.velocity.Multiply(w)
					.Add
					(
						pSOParticle.pbest.Subtract(pSOParticle.position).Multiply(c1 * r1)
					)
					.Add
					(
						gbest.Subtract(pSOParticle.position).Multiply(c2 * r2)
					)
				);
				velocity = rawVelocity.ToVector3();
				particles[j].particleGameObject.transform.position = pSOParticle.position.ToVector3();
				particles[j].particleGameObject.transform.rotation = Quaternion.LookRotation(velocity, Vector3.up);
				
				// update pbest
				if ((selfFunc = pSOParticle.SelfFunc()) < pSOParticle.fbest)
				{
					pSOParticle.pbest = pSOParticle.position;
					pSOParticle.fbest = selfFunc;
				}
			}
			
			w = Math.Clamp(w * weightDecay, weightClamp, 1);
			
			// update gbest
			UpdateGbest();
			yield return new WaitForSeconds(timeDescale * Time.fixedDeltaTime);
		}
		
		/* Deactivate all particle */
		for (int i=0;i<numParticle;i++) particles[i].particleGameObject.SetActive(false);
		/*  */
		
		print($"best {fgbest} at {gbest.ToString()}");
		Vector3 finalPosition = new Vector3((float)gbest[0], (float)gbest[1], (float)gbest[2]);
		transform.position = finalPosition;
		StartCoroutine(VisualDistanceToPlane(visualizeDistanceToPlaneForever));
	}
	
	public void UpdateGbest()
	{
		for (int i=0;i<numParticle;i++)
		{
			if (particles[i].fbest < fgbest)
			{
				gbest = (double[])particles[i].pbest.Clone();
				fgbest = particles[i].fbest;
			}
		}
	}
	
	public PSOParticle[] particles;
	public void Init()
	{
		particles = new PSOParticle[numParticle];
		if (useCustomPositionBounds) for (int i=0;i<dimension;i++) positionBound[i] = customPositionBounds[i].bound;
		else CalculatePositionBound();
		
		double[] posTemp = new double[dimension], vTemp = new double[dimension];
		for (int i=0;i<numParticle;i++)
		{
			for (int j=0;j<dimension;j++)
			{
				posTemp[j] = Random.Range((float)positionBound[j][0], (float)positionBound[j][1]) + potentialCrossedPoint[j];
				vTemp[j] = Random.Range(-1f, 1f);	
			}
			
			particles[i] = new PSOParticle
			(
				dimension,
				posTemp,
				vTemp,
				Function,
				Instantiate(Resources.Load("Particle") as GameObject)
			);
		}
		
		UpdateGbest();
	}
	
	public void CalculatePositionBound()
	{
		double centerToPlaneAverageDistance = 0;
		for (int i=0;i<planes.Count;i++) centerToPlaneAverageDistance += planes[i].CalculateDistance(new Vector3(0, 0, 0));
		centerToPlaneAverageDistance /= planes.Count;
		
		for (int i=0;i<dimension;i++) positionBound[i] = new double[] {-centerToPlaneAverageDistance, centerToPlaneAverageDistance};
	}
}