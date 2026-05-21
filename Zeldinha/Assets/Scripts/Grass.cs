using UnityEngine;

public class Grass : MonoBehaviour
{
    //Particulas da Grama
    public ParticleSystem fxHit;
    private bool isCut;

    void GetHit(int amount)
    {
        //diminuir o tamanho da grama quando receber o ataque
        transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

        if (isCut == false)
        {
            isCut = true;
            fxHit.Emit(Random.Range(5, 10));
        }
    }


}
