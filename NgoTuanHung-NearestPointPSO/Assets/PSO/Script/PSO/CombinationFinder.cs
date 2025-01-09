
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombinationFinder<T>
{
	public int[] combination;
	public List<T> array;
	public int chainNum, l, lastChainIndex;
	public ChainObject[] chains;
	public CombinationFinder(List<T> array, int chainNum)
	{
		this.array = array;
		this.chainNum = chainNum;
		chains = new ChainObject[chainNum];
		combination = new int[chainNum];
		l = array.Count;
		lastChainIndex = chainNum - 1;
	}
	
	public int[] FindCombinationWithCondition(Func<int[], bool> condition)
	{
		InitChainObj();
		while (chains[0].value < l - chainNum + 1)
		{
			while (chains[lastChainIndex].value < l)
			{	
				for (int i=0;i<chainNum;i++) combination[i] = chains[i].value;
				
				if (condition((int[])combination.Clone())) return (int[])combination.Clone();
				
				chains[lastChainIndex].value++;
			}
			
			if (chains[lastChainIndex - 1].value + 1 > l - chainNum + chains[lastChainIndex - 1].chainIndex) {
				UpdateChainBefore(chains[lastChainIndex - 1]);
			} else {
				chains[lastChainIndex - 1].value++;
				chains[lastChainIndex].value = chains[lastChainIndex - 1].value + 1;
			}
		}
		
		return null;
	}
	
	public void UpdateChainBefore(ChainObject chain)
	{
		if (chain.value == l - chainNum + chain.chainIndex) 
		{
			UpdateChainBefore(chains[chain.chainIndex - 1]);
		} 
		else 
		{
			chain.value++;
			UpdateChainAfter(chains[chain.chainIndex + 1]);
		}
	}
	
	public void UpdateChainAfter(ChainObject chain)
	{
		chain.value = chains[chain.chainIndex - 1].value + 1;
		if (chain.chainIndex != lastChainIndex) {
			UpdateChainAfter(chains[chain.chainIndex + 1]);
		}
	}
	
	
	public void InitChainObj()
	{
		for (int i=0;i<chainNum;i++)
		{
			chains[i] = new ChainObject()
			{
				chainIndex = i,
				value = i
			};
		}
	}
}