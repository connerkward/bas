/* 
 * OneEuroFilter.cs
 * Author: Dario Mazzanti (dario.mazzanti@iit.it), 2016
 * 
 * This Unity C# utility is based on the C++ implementation of the OneEuroFilter algorithm by Nicolas Roussel (http://www.lifl.fr/~casiez/1euro/OneEuroFilter.cc)
 * More info on the 1€ filter by Géry Casiez at http://www.lifl.fr/~casiez/1euro/
 *
 */

// Base is mit: https://github.com/DarioMazzanti/OneEuroFilterUnity
// With modifications to make somewhat framerate independent.

using UnityEngine;

class LowPassFilter
{
    float y, a, s;
    bool initialized;

    public void setAlpha(float _alpha)
    {
        if (_alpha <= 0.0f || _alpha > 1.0f)
        {
            Debug.LogError("alpha should be in (0.0., 1.0]");
            return;
        }
        a = _alpha;
    }

    public LowPassFilter(float _alpha, float _initval = 0.0f)
    {
        y = s = _initval;
        setAlpha(_alpha);
        initialized = false;
    }

    public float Filter(float _value)
    {
        float result;
        if (initialized)
            result = a * _value + (1.0f - a) * s;
        else
        {
            result = _value;
            initialized = true;
        }
        y = _value;
        s = result;
        return result;
    }

    public float filterWithAlpha(float _value, float _alpha)
    {
        setAlpha(_alpha);
        return Filter(_value);
    }

    public bool hasLastRawValue()
    {
        return initialized;
    }

    public float lastRawValue()
    {
        return y;
    }

};

// -----------------------------------------------------------------

public class OneEuroFilter
{
    public const float FREQUENCY_MULTIPLIER = .01f;

    float baseFreq;
    float mincutoff;
    float beta;
    float dcutoff;
    LowPassFilter x;
    LowPassFilter dx;

    // currValue contains the latest value which has been successfully filtered
    // prevValue contains the previous filtered value
    public float currValue { get; protected set; }
    public float prevValue { get; protected set; }

    // Calculate alpha based on cutoff and deltaTime using base frequency
    float alpha(float _cutoff, float deltaTime)
    {
        // Scale time by baseFreq to make it framerate independent
        float te = 1.0f / baseFreq * deltaTime;
        float tau = 1.0f / (2.0f * Mathf.PI * _cutoff);
        return 1.0f / (1.0f + tau / te);
    }

    void setMinCutoff(float _mc)
    {
        if (_mc <= 0.0f)
        {
            Debug.LogError("mincutoff should be > 0");
            return;
        }
        mincutoff = _mc;
    }

    void setBeta(float _b)
    {
        beta = _b;
    }

    void setDerivateCutoff(float _dc)
    {
        if (_dc <= 0.0f)
        {
            Debug.LogError("dcutoff should be > 0");
            return;
        }
        dcutoff = _dc;
    }

    public OneEuroFilter(float _baseFreq, float _mincutoff = 1.0f, float _beta = 0.0f, float _dcutoff = 1.0f)
    {
        baseFreq = _baseFreq;
        setMinCutoff(_mincutoff);
        setBeta(_beta);
        setDerivateCutoff(_dcutoff);
        x = new LowPassFilter(alpha(mincutoff, 1.0f / baseFreq)); // Initialize with scaled default time
        dx = new LowPassFilter(alpha(dcutoff, 1.0f / baseFreq));
        currValue = 0.0f;
        prevValue = currValue;
    }

    public void UpdateParams(float _baseFreq, float _mincutoff = 1.0f, float _beta = 0.0f, float _dcutoff = 1.0f)
    {
        baseFreq = _baseFreq;
        setMinCutoff(_mincutoff);
        setBeta(_beta);
        setDerivateCutoff(_dcutoff);
    }

    public float Filter(float value, float deltaTime)
    {
        prevValue = currValue;

        // Estimate the current variation per second
        float dvalue = x.hasLastRawValue() ? (value - x.lastRawValue()) / deltaTime : 0.0f;
        float edvalue = dx.filterWithAlpha(dvalue, alpha(dcutoff, deltaTime));

        // Use it to update the cutoff frequency
        float cutoff = mincutoff + beta * Mathf.Abs(edvalue);

        // Filter the given value
        currValue = x.filterWithAlpha(value, alpha(cutoff, deltaTime));

        return currValue;
    }
}

// Dont use generics like in original solution for performance reasons
public class OneEuroFilterQuaternion
{
    private OneEuroFilter[] filters = new OneEuroFilter[4];

    public OneEuroFilterQuaternion(float frequency)
    {
        filters[0] = new OneEuroFilter(frequency * OneEuroFilter.FREQUENCY_MULTIPLIER);
        filters[1] = new OneEuroFilter(frequency * OneEuroFilter.FREQUENCY_MULTIPLIER);
        filters[2] = new OneEuroFilter(frequency * OneEuroFilter.FREQUENCY_MULTIPLIER);
        filters[3] = new OneEuroFilter(frequency * OneEuroFilter.FREQUENCY_MULTIPLIER);
    }

    public void UpdateParams(float _freq, float _mincutoff = 1.0f, float _beta = 0.0f, float _dcutoff = 1.0f)
    {
        filters[0].UpdateParams(_freq * OneEuroFilter.FREQUENCY_MULTIPLIER, _mincutoff, _beta, _dcutoff);
        filters[1].UpdateParams(_freq * OneEuroFilter.FREQUENCY_MULTIPLIER, _mincutoff, _beta, _dcutoff);
        filters[2].UpdateParams(_freq * OneEuroFilter.FREQUENCY_MULTIPLIER, _mincutoff, _beta, _dcutoff);
        filters[3].UpdateParams(_freq * OneEuroFilter.FREQUENCY_MULTIPLIER, _mincutoff, _beta, _dcutoff);
    }

    public Quaternion Filter(Quaternion input, float deltaTime)
    {
        Quaternion output = Quaternion.identity;

        // Workaround that take into account that some input device sends
        // quaternion that represent only a half of all possible values.
        // this piece of code does not affect normal behaviour (when the
        // input use the full range of possible values).
        if (Vector4.SqrMagnitude(new Vector4(filters[0].currValue, filters[1].currValue, filters[2].currValue, filters[3].currValue).normalized
            - new Vector4(input[0], input[1], input[2], input[3]).normalized) > 2)
        {
            input = new Quaternion(-input.x, -input.y, -input.z, -input.w);
        }

        output[0] = filters[0].Filter(input[0], deltaTime);
        output[1] = filters[1].Filter(input[1], deltaTime);
        output[2] = filters[2].Filter(input[2], deltaTime);
        output[3] = filters[3].Filter(input[3], deltaTime);

        return output;
    }
}
public class OneEuroFilterVector3
{
    private OneEuroFilter[] filters = new OneEuroFilter[3];

    public OneEuroFilterVector3(float frequency)
    {
        filters[0] = new OneEuroFilter(frequency * OneEuroFilter.FREQUENCY_MULTIPLIER);
        filters[1] = new OneEuroFilter(frequency * OneEuroFilter.FREQUENCY_MULTIPLIER);
        filters[2] = new OneEuroFilter(frequency * OneEuroFilter.FREQUENCY_MULTIPLIER);
    }

    public void UpdateParams(float _freq, float _mincutoff = 1.0f, float _beta = 0.0f, float _dcutoff = 1.0f)
    {
        filters[0].UpdateParams(_freq * OneEuroFilter.FREQUENCY_MULTIPLIER, _mincutoff, _beta, _dcutoff);
        filters[1].UpdateParams(_freq * OneEuroFilter.FREQUENCY_MULTIPLIER, _mincutoff, _beta, _dcutoff);
        filters[2].UpdateParams(_freq * OneEuroFilter.FREQUENCY_MULTIPLIER, _mincutoff, _beta, _dcutoff);
    }

    public Vector3 Filter(Vector3 input, float deltaTime)
    {
        Vector3 output = Vector3.zero;

        output[0] = filters[0].Filter(input[0], deltaTime);
        output[1] = filters[1].Filter(input[1], deltaTime);
        output[2] = filters[2].Filter(input[2], deltaTime);

        return output;
    }
}
public class OneEuroFilterFloat
{
    private OneEuroFilter filter;

    public OneEuroFilterFloat(float frequency)
    {
        filter = new OneEuroFilter(frequency * OneEuroFilter.FREQUENCY_MULTIPLIER);
    }

    public void UpdateParams(float _freq, float _mincutoff = 1.0f, float _beta = 0.0f, float _dcutoff = 1.0f)
    {
        filter.UpdateParams(_freq, _mincutoff, _beta, _dcutoff);
    }

    public float Filter(float input, float deltaTime)
    {
        return filter.Filter(input, deltaTime);
    }
}

/*
// this class instantiates an array of OneEuroFilter objects to filter each component of Vector2, Vector3, Vector4 or Quaternion types
public class OneEuroFilter<T> where T : struct
{
	// containst the type of T
	Type type;
	// the array of filters
	OneEuroFilter[] oneEuroFilters;

	// filter parameters
	public float freq {get; protected set;}
	public float mincutoff {get; protected set;}
	public float beta {get; protected set;}
	public float dcutoff {get; protected set;}

	// currValue contains the latest value which have been succesfully filtered
	// prevValue contains the previous filtered value
	public T currValue {get; protected set;}
	public T prevValue {get; protected set;}

	// initialization of our filter(s)
	public OneEuroFilter(float _freq, float _mincutoff = 1.0f, float _beta = 0.0f, float _dcutoff = 1.0f)
	{
		type = typeof(T);
		currValue = new T();
		prevValue = new T();

		freq = _freq* OneEuroFilter.FREQUENCY_MULTIPLIER;
		mincutoff = _mincutoff;
		beta = _beta;
		dcutoff = _dcutoff;

		if(type == typeof(Vector2))
			oneEuroFilters = new OneEuroFilter[2];

		else if(type == typeof(Vector3))
			oneEuroFilters = new OneEuroFilter[3];

		else if(type == typeof(Vector4) || type == typeof(Quaternion))
			oneEuroFilters = new OneEuroFilter[4];
		else
		{
			Debug.LogError(type + " is not a supported type");
			return;
		}

		for(int i = 0; i < oneEuroFilters.Length; i++)
			oneEuroFilters[i] = new OneEuroFilter(freq, mincutoff, beta, dcutoff);		
	}

	// updates the filter parameters
	public void UpdateParams(float _freq, float _mincutoff = 1.0f, float _beta = 0.0f, float _dcutoff = 1.0f)
	{
		freq = _freq* OneEuroFilter.FREQUENCY_MULTIPLIER;
		mincutoff = _mincutoff;
		beta = _beta;
		dcutoff = _dcutoff;
		
		for(int i = 0; i < oneEuroFilters.Length; i++)
			oneEuroFilters[i].UpdateParams(freq, mincutoff, beta, dcutoff);
	}


	// filters the provided _value and returns the result.
	// Note: a timestamp can also be provided - will override filter frequency.
	public T Filter<U>(U _value, float deltaTime) where U : struct
	{
		prevValue = currValue;
		
		if(typeof(U) != type)
		{
			Debug.LogError("WARNING! " + typeof(U) + " when " + type + " is expected!\nReturning previous filtered value" );
			currValue = prevValue;
	
			return (T) Convert.ChangeType(currValue, typeof(T));
		}

		if(type == typeof(Vector2))
		{
			Vector2 output = Vector2.zero;
			Vector2 input = (Vector2) Convert.ChangeType(_value, typeof(Vector2));

			for(int i = 0; i < oneEuroFilters.Length; i++)
				output[i] = oneEuroFilters[i].Filter(input[i], deltaTime);

			currValue = (T) Convert.ChangeType(output, typeof(T));
		}

		else if(type == typeof(Vector3))
		{
			Vector3 output = Vector3.zero;
			Vector3 input = (Vector3) Convert.ChangeType(_value, typeof(Vector3));

			for(int i = 0; i < oneEuroFilters.Length; i++)
				output[i] = oneEuroFilters[i].Filter(input[i], deltaTime);

			currValue = (T) Convert.ChangeType(output, typeof(T));
		}

		else if(type == typeof(Vector4))
		{
			Vector4 output = Vector4.zero;
			Vector4 input = (Vector4) Convert.ChangeType(_value, typeof(Vector4));

			for(int i = 0; i < oneEuroFilters.Length; i++)
				output[i] = oneEuroFilters[i].Filter(input[i], deltaTime);

			currValue = (T) Convert.ChangeType(output, typeof(T));
		}

		else
		{
			Quaternion output = Quaternion.identity;
			Quaternion input = (Quaternion) Convert.ChangeType(_value, typeof(Quaternion));
            
            // Workaround that take into account that some input device sends
            // quaternion that represent only a half of all possible values.
            // this piece of code does not affect normal behaviour (when the
            // input use the full range of possible values).
            if (Vector4.SqrMagnitude(new Vector4(oneEuroFilters[0].currValue, oneEuroFilters[1].currValue, oneEuroFilters[2].currValue, oneEuroFilters[3].currValue).normalized
                - new Vector4(input[0], input[1], input[2], input[3]).normalized) > 2)
            {
                input = new Quaternion(-input.x, -input.y, -input.z, -input.w);
            }

			for(int i = 0; i < oneEuroFilters.Length; i++)
				output[i] = oneEuroFilters[i].Filter(input[i], deltaTime);

			currValue = (T) Convert.ChangeType(output, typeof(T));
		}

		return (T) Convert.ChangeType(currValue, typeof(T));
	}
}
*/