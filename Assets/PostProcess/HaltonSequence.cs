// HaltonSequence.cs

using UnityEngine;
using System.Collections;

// converted to unity c# by http://unitycoder.com/blog
// original source: http://www.openprocessing.org/sketch/1920

public class HaltonSequence
{
	public Vector3 m_CurrentPos = new Vector3(0.0f,0.0f,0.0f);
	long m_Base2 = 0;
	long m_Base3 = 0;
	long m_Base5 = 0;
	
	public long Increment()
	{
		float fOneOver3 = 1.0f/3.0f;
		float fOneOver5 = 1.0f/5.0f;
		
		long oldBase2 = m_Base2;
		m_Base2++;
		long diff = m_Base2 ^ oldBase2;
		
		float s = 0.5f;
		
		do
		{
			if ((oldBase2 & 1) == 1)
				m_CurrentPos.x -= s;
			else
				m_CurrentPos.x += s;
			
			s *= 0.5f;
			
			diff = diff >> 1;
			oldBase2 = oldBase2 >> 1;
		}
		while (diff > 0);
		
		long bitmask = 0x3;
		long bitadd  = 0x1;
		s = fOneOver3;
		
		m_Base3++;
		
		while (true)
		{
			if ((m_Base3 & bitmask) == bitmask)
			{
				m_Base3 += bitadd;
				m_CurrentPos.y -= 2 * s;
				
				bitmask = bitmask << 2;
				bitadd  = bitadd  << 2;
				
				s *= fOneOver3;
			}
			else
			{
				m_CurrentPos.y += s;
				break;
			}
		};
		bitmask = 0x7;
		bitadd  = 0x3;
		long dmax = 0x5;
		
		s = fOneOver5;
		
		m_Base5++;
		
		while (true)
		{
			if ((m_Base5 & bitmask) == dmax)
			{
				m_Base5 += bitadd;
				m_CurrentPos.z -= 4 * s;
				
				bitmask = bitmask << 3;
				dmax = dmax << 3;
				bitadd  = bitadd  << 3;
				
				s *= fOneOver5;
			}
			else
			{
				m_CurrentPos.z += s;
				break;
			}
		};
		
		return m_Base2;
	}
	
	public void Reset()
	{
		m_CurrentPos.x = 0.0f;
		m_CurrentPos.y = 0.0f;
		m_CurrentPos.z = 0.0f;
		m_Base2 = 0;
		m_Base3 = 0;
		m_Base5 = 0;
	}
}