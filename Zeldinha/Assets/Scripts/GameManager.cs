using System.Collections;
using TMPro;
using UnityEngine;


//Criaos 5 alertas gerenciar estados dos inimigos
public enum enemyState
{
    IDLE,ALERT, PATROL, FOLLOW, FURY
}

//Gerenciar o nosso jogo
public enum GameState
{
        GAMEPLAY, DIE
}
public class GameManager : MonoBehaviour
{
    
    public GameState gameState;

    [Header("Slime IA")]
    //------------Patrol e Idle
    public Transform[] slimeWayPoints;//posições
    public float slimePatrolWaitTime;//tempo de espera PATROL
    public float slimeIdleWaitTime;//tempo de espera IDLE
    //-------------Fury  
    [Header("Player")]
    public Transform player;//Buscar a Posição do player no FURY
    public float slimeDistanceAtack = 2.3f;//Distancia que o inimigo para para atacar o player
    //----------Gemas
    [Header("Gemas")]
    public int gemas = 0;
    public TextMeshProUGUI txtGemas;
    public GameObject gemaPrefab;
    public int percDrop = 25; //chance geral de dropar o item

    //-------------Alerta
    public float slimeAlertTime = 3f;

    //-------------Olhar
    public float slimeLookAtSpeed = 5f;

    [Header("Chuva")]
    public ParticleSystem chuva;
    public ParticleSystem.EmissionModule rainModule;
    public int chuvaFim;//Máximo de chuva
    public int chuvaAumento;//Aumento da chuva
    public float chuvaTempo;//Tempo da chuva
    public Light iluminacao;

    private void Start()
    {
        rainModule = chuva.emission;
        txtGemas.text = gemas.ToString();
    }
    public void onOfRain(bool isRain)
    {
        StopCoroutine("RainManager");
        StartCoroutine("RainManager", isRain);
        StopCoroutine("Anoitece");
        StartCoroutine("Anoitece", isRain);

    }

    IEnumerator RainManager(bool isRain)
    {
        switch (isRain)
        {
            case true://Aumenta a chuva
                for (float r = rainModule.rateOverTime.constant; r < chuvaFim; r += chuvaAumento)
                {
                    rainModule.rateOverTime = r;
                    yield return new WaitForSeconds(chuvaTempo);
                }
                rainModule.rateOverTime = chuvaFim;
                break;

            case false://Diminui a chuva
                for (float r = rainModule.rateOverTime.constant; r > 0; r -= chuvaAumento)
                {
                    rainModule.rateOverTime = r;
                    yield return new WaitForSeconds(chuvaTempo);
                }
                rainModule.rateOverTime = 0;
                break;
        }
    }

    IEnumerator Anoitece(bool isRain)
        {
            switch (isRain)
            {
                case true:
                    for (float intensidade = iluminacao.intensity; intensidade > 0f; intensidade -= 1 * Time.deltaTime/2)
                    {
                        iluminacao.intensity = (intensidade);
                        yield return new WaitForEndOfFrame();
                    }
                    iluminacao.intensity = 0f;

                    break;

                case false:
                    for (float intensidade = iluminacao.intensity; intensidade <= 2.5f; intensidade += 1 * Time.deltaTime/2)
                    {
                        iluminacao.intensity = (intensidade);
                        yield return new WaitForEndOfFrame();
                    }
                    iluminacao.intensity = 2.5f;
                    break;                        
                }
            }

    //Estado do Game
    public void ChangeGameState(GameState state)
    {
        gameState = state;
    }

    //Pegar gemas
    public void setGemas(int amount)
    {
        gemas += amount;
        txtGemas.text = gemas.ToString();
    }

    public bool ChanceDropar(int chance)
    {
        int randChance = Random.Range(0, 100);//Recebe valor de 0 a 100 randomico
        bool retorno = randChance <= chance?true:false;//caso esse valor for menor ou igual a 25
        return retorno;//retorna verdadeiro para dropar o item
    }
}
