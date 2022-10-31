using System.Collections;
using Cinemachine;
using UnityEngine;
using Cinemachine.Utility;
using Unity.Mathematics;
using UnityEngine.VFX;
using Event = AK.Wwise.Event;

public class KiteController : MonoBehaviour
{
    [Header("Windways")]
    private WindWaysManager _windWaysManager;
    public bool insideWindWay;
    public float buildUp;
    public float buildUpMultiplier = 0.1f;
    public float buildDownMultiplier = 0.4f;
    public bool freezeBuildDown = false;
    public float magnetForce = 10f;
    public bool turboFreezeBuildDown = false;
    public float cheatForce = 10.0f;

    [Header("Haptics")] public HapticsProfile insideWindwayProfile;
    public HapticsProfile closeToWindZoneProfile;
    public HapticsProfile collisionProfile;

    [Header("Physics")]
    public float shiftingForce = 5f;
    public float upWardsForce = 5f;
    public float tilting = 1.5f;
    [HideInInspector] public bool turbulent;
	[HideInInspector] public float velocity;

    [Space] [Header("Rail & Wind")] public CinemachinePath kaiTrack;
    public CinemachineDollyCart kaiCart;
    public CinemachinePath araTrack;
    public CinemachineDollyCart araCart;
    public CinemachineDollyCart kaiProjection;
    public float refocusingForce;
    public float hardRefocusForce;
    [SerializeField] private int samplesSize = 255;
    public float turbulenceAmplitude = 5f;
    public float turbulenceSize = 1000f;
    public ForceMode turbulenceMode = ForceMode.Force;
    public AnimationCurve curve;
    public float curveSeverity = 1.5f;
    public float distanceUntilSevere = 10f;
    
    [Space] [Header("Audio Parameters")]
    [HideInInspector] public float movementIntensity;
    public Event collisionEvent;
    public Event repulsionWhoosh;
    public Event airflow_in;
    public Event airflow_out;
    [HideInInspector] public float distanceToCenter;
    public float movementIntensityMultiplier;
    private bool alreadyPlayed;
    [HideInInspector] public Vector3 vectorTowardsCenter;
    private float previousDistanceToCenter;
    private InputRecorder inputs;
  
	[Header("Other")]
    [SerializeField] private Transform ccc;
    public Transform kiteTransform;
    public Rigidbody kiteRigidbody;
	public GameObject kite1;
    public GameObject kite2;
	public KiteLine rightString;
    public KiteLine leftString;
	public ArchVFX archVFXEmitter;
    private GameObject listener;
    private Transform kite2left;
    private Transform ktie2right;

	private float left;
    private float right;

    public void Start()
    {
        if ((inputs = FindObjectOfType<InputRecorder>()) == null)
            Debug.LogError("Could not find InputRecorder", gameObject);
        listener = Camera.main.gameObject;
        if ((_windWaysManager = FindObjectOfType<WindWaysManager>()) == null)
            Debug.LogError("Could not find WindWaysManager", gameObject);
    }

    void Update()
    {
        buildUp = Mathf.Clamp(buildUp, 0, 6);
        left = inputs.left;
        right = inputs.right;
    }

    private void FixedUpdate()
    {
        #region InputForces
        
        movementIntensity += (Mathf.Abs(left - right) - 0.5f) * Time.deltaTime * movementIntensityMultiplier;
        movementIntensity = Mathf.Clamp(movementIntensity, buildUp * 5, 40 + buildUp * 10);
        float upForce = (left * right) * this.upWardsForce;
        float leftForce = left * shiftingForce;
        float rightForce = right * shiftingForce;
        var cccTransform = ccc.transform;
        var cccRight = cccTransform.right;
        var rightForces = rightForce * cccRight;
        var leftForces = leftForce * -cccRight;
        var upForces = upForce * cccTransform.up;
        Vector3 localVelocity = cccTransform.InverseTransformDirection(kiteRigidbody.velocity);
        localVelocity.z = 0;
        kiteRigidbody.velocity = cccTransform.TransformDirection(localVelocity);
        var rotations =
            new Vector3(localVelocity.y, localVelocity.x / 2f, localVelocity.x) * -tilting;
        velocity = Quaternion.Angle(Quaternion.identity, Quaternion.Euler(rotations));
        kiteTransform.localRotation = ccc.transform.localRotation * Quaternion.Euler(rotations);
        kiteRigidbody.AddForce(leftForces, ForceMode.Force);
        kiteRigidbody.AddForce(rightForces, ForceMode.Force);
        kiteRigidbody.AddForce(upForces,  ForceMode.Force);

        #endregion

        #region Refocusing
        
        var cartPosition = (kaiCart.m_Path.EvaluatePositionAtUnit(kaiCart.m_Position, kaiCart.m_PositionUnits));
        var position = kiteTransform.position;
        vectorTowardsCenter = cartPosition - position;
        previousDistanceToCenter = this.distanceToCenter;
        this.distanceToCenter = vectorTowardsCenter.magnitude;
        if (this.distanceToCenter >= distanceUntilSevere &&
            previousDistanceToCenter < distanceUntilSevere)
        {
            var localDir = cccTransform.InverseTransformPoint(kiteTransform.position);
            var rightRatio =  Mathf.Sqrt(Mathf.InverseLerp( - this.distanceToCenter,
                 this.distanceToCenter, localDir.x));
            var leftRatio = Mathf.Sqrt(Mathf.InverseLerp( this.distanceToCenter,
                 - this.distanceToCenter, localDir.x));
            inputs.Vibrate(leftRatio, rightRatio, closeToWindZoneProfile);
            repulsionWhoosh.Post(listener);
        }
        kiteRigidbody.AddForce((cartPosition - position) * refocusingForce * curve.Evaluate(this.distanceToCenter/this.distanceUntilSevere));

        #endregion
        
        #region WindWays
        
        float minDist = float.MaxValue;
        int[] ids = new int[2];
		{
        	var w = araTrack.m_Waypoints;
			var length = araTrack.m_Waypoints.Length;
        	for(int i = 0; i<length; i++)
			{
            	float dist = Vector3.Distance(kiteTransform.position, w[i].position + araTrack.transform.position);
            	if (!(dist < minDist)) continue;
            	ids[1] = ids[0];
            	ids[0] = i;
            	minDist = dist;
        	}
		}
        if (ids[1] > ids[0])
			(ids[0], ids[1]) = (ids[1], ids[0]);

        int mean = (ids[0] + ids[1]) / 2;
        var closest = araTrack.FromPathNativeUnits((float)FindClosestPoint(kiteTransform.position, mean, Mathf.Abs((ids[0] - ids[1])/2) + 1, 1000), CinemachinePathBase.PositionUnits.Distance);
        kaiProjection.m_Position = closest;
        WindWay closestWindWay;
        Vector3 center = Vector3.zero;
        float distanceToCenter = 0;
        if ((closestWindWay = _windWaysManager.GetCorrespondingWindway(closest)) != null)
        {
            center = araTrack.EvaluatePositionAtUnit(closest, CinemachinePathBase.PositionUnits.Distance);
            distanceToCenter = Vector3.Distance(kiteTransform.position, center);
            insideWindWay = closestWindWay.IsInside(distanceToCenter);
            var buildMult = insideWindWay ? 1 * buildUpMultiplier : -1 * buildDownMultiplier * (turboFreezeBuildDown ? 0 : 1);
            buildUp += buildMult * Time.deltaTime;
            if (insideWindWay && !alreadyPlayed)
            {
                airflow_in.Post(Camera.main.gameObject);
                alreadyPlayed = true;
                inputs.Vibrate(1, 1, insideWindwayProfile);
            }
        }
        else
        {
            insideWindWay = false;
        }
        if (!insideWindWay)
        {
            alreadyPlayed = false;
            airflow_out.Post(Camera.main.gameObject);
        }
        buildUp = Mathf.Clamp(buildUp, 0f, 6f);
        if (closestWindWay != null && insideWindWay)
        {
            kiteRigidbody.AddForce((center - kiteTransform.position) * (magnetForce * Mathf.Log((distanceToCenter/closestWindWay.radius) + 1f, 5)));
            
        }
		else if (closestWindWay != null && closestWindWay.IsInside(distanceToCenter * _windWaysManager.cheatWindWaySizeRatio))
        {
            kiteRigidbody.AddForce((center - kiteTransform.position) * cheatForce);
        }
            
        #endregion

        #region Turbulence

        var turbulence = new Vector2(Mathf.PerlinNoise((position.x + Time.deltaTime) * turbulenceSize, (position.y + Time.deltaTime) * turbulenceSize) - 0.5f, Mathf.PerlinNoise(100 + (position.x + Time.deltaTime) * turbulenceSize, 100 + (position.y + Time.deltaTime) * turbulenceSize) - 0.5f);
        kiteRigidbody.AddForce(turbulence.x * turbulenceAmplitude, turbulence.y * turbulenceAmplitude, 0, ForceMode.Force);

        #endregion
        
        Vector3 localVelocity2 = cccTransform.InverseTransformDirection(kiteRigidbody.velocity);
        localVelocity2.z = 0;
        kiteRigidbody.velocity = cccTransform.TransformDirection(localVelocity2);
        
    }

    
    public double FindClosestPoint(
        Vector3 p, int startSegment, int searchRadius, int stepsPerSegment)
    {
        double start = araTrack.MinPos;
        double end = araTrack.MaxPos;
        if (searchRadius >= 0)
        {
            int r = Mathf.FloorToInt(Mathf.Min(searchRadius, (float)(end - start) / 2f));
            start = startSegment - r;
            end = startSegment + r + 1;
            if (!araTrack.Looped)
            {
                start = Mathf.Max((float)start, araTrack.MinPos);
                end = Mathf.Min((float)end, araTrack.MaxPos);
            }
        }
        stepsPerSegment = Mathf.RoundToInt(Mathf.Clamp(stepsPerSegment, 1f, 1000f));
        double stepSize = 1f / stepsPerSegment;
        double bestPos = startSegment;
        double bestDistance = double.MaxValue;
        int iterations = (stepsPerSegment == 1) ? 1 : 3;
        for (int i = 0; i < iterations; ++i)
        {
            Vector3 v0 = araTrack.EvaluatePosition((float)start);
            for (double f = start + stepSize; f <= end; f += stepSize)
            {
                Vector3 v = araTrack.EvaluatePosition((float)f);
                double t = (float) p.ClosestPointOnSegment(v0, v);
                double d = Vector3.SqrMagnitude(p - Vector3.Lerp(v0, v, (float)t));
                if (d < bestDistance)
                {
                    bestDistance = d;
                    bestPos = f - (1 - t) * stepSize;
                }
                v0 = v;
            }
            start = bestPos - stepSize;
            end = bestPos + stepSize;
            stepSize /= stepsPerSegment;
        }
        return bestPos;
    }


    public void ChangeTurbulenceMode(ForceMode mode)
    {
        turbulenceMode = mode;
    }

    public void SetNoisyModeForXSeconds(float time)
    {
        turbulent = true;
        turbulenceMode = ForceMode.VelocityChange;
        turbulenceAmplitude *= 10; 
        StartCoroutine(SetBackDefaultMode(time));

    }

    public IEnumerator SetBackDefaultMode(float time)
    {
        yield return new WaitForSeconds(time);
        turbulent = false;
        turbulenceMode = ForceMode.Force;
    }

    public void SetBuildUp(float buildUpValue)
    {
        if (buildUp < buildUpValue) buildUp = buildUpValue;
    }

    public void TeleportOffset(float addedPosition)
    {
        kaiCart.m_Position += addedPosition;
        araCart.m_Position += addedPosition;
    }

    public void SwitchKite()
    {
        kite1.SetActive(false);
        kite2.SetActive(true);
        rightString.anchor = ktie2right;
        leftString.anchor = kite2left;
    }

}
