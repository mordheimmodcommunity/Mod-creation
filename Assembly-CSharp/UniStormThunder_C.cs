using UnityEngine;

public class UniStormThunder_C : MonoBehaviour
{
	public float flashLength;

	public float lightningStrikeOdds;

	public float minIntensity;

	public float maxIntensity;

	public float minRotationAmount;

	public float maxRotationAmount;

	public float minRotationAmountX;

	public float maxRotationAmountX;

	public float minRotationAmountZ;

	public float maxRotationAmountZ;

	public GameObject lightningBolt1;

	public GameObject lightningBolt2;

	public AudioClip[] lightningSound;

	public float lastTime;

	public Light lightSource;

	private float random;

	private void Start()
	{
		lightSource = GetComponent<Light>();
	}

	private void Awake()
	{
		random = Random.Range(0f, 65535f);
	}

	private void Update()
	{
		if (Time.time - lastTime > flashLength)
		{
			if (Random.value > lightningStrikeOdds)
			{
				Object.Instantiate(lightningBolt1);
				Object.Instantiate(lightningBolt2);
				float t = Mathf.PerlinNoise(random, Time.time);
				float yAngle = Mathf.Lerp(minRotationAmount, maxRotationAmount, t);
				base.transform.Rotate(0f, yAngle, 0f);
				float t2 = Mathf.PerlinNoise(random, Time.time);
				float num = Mathf.Lerp(minRotationAmountX, maxRotationAmountX, t2);
				base.transform.Rotate(0f, num, 0f);
				float t3 = Mathf.PerlinNoise(random, Time.time);
				float num2 = Mathf.Lerp(minRotationAmountZ, maxRotationAmountZ, t3);
				base.transform.Rotate(0f, num2, 0f);
				base.transform.eulerAngles = new Vector3(0f, num, 0f);
				base.transform.eulerAngles = new Vector3(0f, 0f, num2);
				float t4 = Mathf.PerlinNoise(random, Time.time);
				GetComponent<Light>().intensity = Mathf.Lerp(minIntensity, maxIntensity, t4);
				GetComponent<Light>().enabled = true;
				GetComponent<AudioSource>().PlayOneShot(lightningSound[Random.Range(0, lightningSound.Length)]);
			}
			else
			{
				GetComponent<Light>().enabled = false;
			}
			lastTime = Time.time;
		}
	}
}
