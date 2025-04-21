using UnityEngine;

public class WaterManager : MonoBehaviour
{
    private const int BOUNDS_SIZE = 100;
    
    [SerializeField] private Material _waterMaterial;
    [SerializeField] private Transform _waterObject;

    private Vector4 _waveA;
    private Vector4 _waveB ;
    private float _attenuationStrength;
    
    //public int gridSizeX = 20;
    //public int gridSizeZ = 20;
    //public float gridStep = 0.5f;

    public void Initialize()
    {
        _waveA = _waterMaterial.GetVector("_WaveA");
        _waveB = _waterMaterial.GetVector("_WaveB");
        _attenuationStrength = _waterMaterial.GetFloat("_WaveAttenuationStrength");
    }
/*
    private void OnDrawGizmos()
    {
        Vector3 waterScale = _waterObject.localScale;
        Vector3 waterPos = _waterObject.position;

        Vector3 origin = waterPos - new Vector3(gridSizeX * gridStep * 0.5f, 0, gridSizeZ * gridStep * 0.5f);
        Gizmos.color = Color.cyan;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int z = 0; z < gridSizeZ; z++)
            {
                Vector3 p1 = origin + new Vector3(x * gridStep, 0, z * gridStep);
                Vector3 p2 = origin + new Vector3((x + 1) * gridStep, 0, z * gridStep);
                Vector3 p3 = origin + new Vector3(x * gridStep, 0, (z + 1) * gridStep);

                p1.y = GetWaveHeight(p1);
                p2.y = GetWaveHeight(p2);
                p3.y = GetWaveHeight(p3);

                Gizmos.DrawLine(p1, p2);
                Gizmos.DrawLine(p1, p3);
            }
        }
    }
    */
    
    

    public float GetWaveHeight(Vector3 worldPos)
    {
        Vector3 localPos = worldPos - _waterObject.position;
        Vector3 scale = _waterObject.localScale;
        
        float uvY = (localPos.z /(scale.z * BOUNDS_SIZE) + 0.5f);
        float distFromCenter = Mathf.Abs(uvY - 0.5f) * 2.0f;
        float waveAttenuation = Mathf.Pow(Mathf.Clamp01(1.0f - distFromCenter), _attenuationStrength);
        
        localPos = new Vector3(localPos.x / scale.x, localPos.y / scale.y, localPos.z / scale.z);

        float heightA = GerstnerWave(_waveA, localPos, waveAttenuation);
        float heightB = GerstnerWave(_waveB, localPos, waveAttenuation);

        return (heightA + heightB) * scale.y + _waterObject.position.y;
    }

    private float GerstnerWave(Vector4 wave, Vector3 p, float attenuation)
    {
        float steepness = wave.z * attenuation;
        float wavelength = wave.w;
        float k = 2 * Mathf.PI / wavelength;
        float c = Mathf.Sqrt(9.8f / k);
        Vector2 d = new Vector2(wave.x, wave.y).normalized;

        float f = k * (Vector2.Dot(d, new Vector2(p.x, p.z)) - c * Time.time);
        float a = steepness / k;

        return a * Mathf.Sin(f);
    }
}