using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using static Unity.Burst.Intrinsics.X86;

public class SlimeIA : MonoBehaviour
{
  #region Variáveis

    [Header("Estado Inimigo")]
    public enemyState state;//Vem do GameManager

    [Header("Animações")]
    private Animator m_Animator;
    public int hp;
    private bool isDie;

    [Header("Movimentação")]
    private GameManager _gameManager;//Script Game Maneger obj Instance
    private NavMeshAgent agent;//Malha de navegação
    private Vector3 destination;//Movimento
    private int idWayPoint;//Pontos de espera rotas
    public bool isRunningCoroutine = false; // Controle para evitar múltiplas cor
    
    //ALERT
    public bool isPlayerVisible;
    //Ataque
    public bool isAtack;
    //fury
    public float atackDelay;

  #endregion

    void Start()
    {
        _gameManager = FindFirstObjectByType<GameManager>();
        m_Animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
       // hp = 30;
        if (agent == null)
        {
            Debug.LogError("NavMeshAgent não encontrado!");
            return;
        }
        isDie = false;
        agent.isStopped = false;

        //ChangeState(state);---------------------------------------Remova
        isRunningCoroutine = true;
    }

    private void LateUpdate()
    {

        if (!isDie)
            if (_gameManager.gameState != GameState.DIE)
            {
                StateManager();
            }
            else
            {
                StopAllCoroutines();
                ChangeState(enemyState.IDLE);
                StateManager();
            }

    }
    //--------------------------Efeito dano e morte inimigo
    //Efeito de morte do inimigo
    IEnumerator Died()
    {
        isDie = true;
        agent.isStopped = true;

        float duration = 2.3f;
        float elapsed = 0f;
        Vector3 initialScale = transform.localScale;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            transform.localScale = Vector3.Lerp(initialScale, Vector3.zero, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        //Chama o metodo do game manager para dropar as gemas 
        //Porcentagem pode ser passa manualmente se quiser aumentar ou diminuir o drop
        if (_gameManager.ChanceDropar(_gameManager.percDrop))
        {
            Instantiate(_gameManager.gemaPrefab, transform.position, _gameManager.gemaPrefab.transform.rotation);
        }

        Destroy(gameObject);
    }

    //Tira Vida do Inimigo e faz animação de morte
    void GetHit(int amount)
    {
        if (isDie) return;

        hp = hp - amount;

        if (hp > 0)
        {
            ChangeState(enemyState.FURY);//Persegue o jogador quando atingido
            m_Animator.SetTrigger("GetHitTrigger");//Animação de dano
        }
        else
        {
            m_Animator.SetTrigger("DieTrigger");//Animação de Morte
            StartCoroutine(Died());
        }
    }


    //Espera determinado tempo para fazer o sorteio das rotinas -> Faz sorteio para mudar rotinas
    //--------------------------IDLE
    IEnumerator IDLE()
    {

        if (_gameManager.gameState == GameState.DIE)
        {
            m_Animator.SetBool("isAlert", false);//Alerta animação
            m_Animator.SetBool("isWalk", false);//Alerta animação
            m_Animator.SetBool("isHappy", true);
            isPlayerVisible = false;
        }
        else
        {

            yield return new WaitForSeconds(_gameManager.slimeIdleWaitTime);
            TrocarEstado(50);
            isRunningCoroutine = true;

        }
    }

    //---------------------------PATROL
    IEnumerator PATROL()
    {
        //EscolherNovoDestino();
        yield return new WaitUntil(() => agent.remainingDistance <= 0.2f);
        TrocarEstado(20);
    }
    //Método Movimentação Patrulha nos wayPoints

    void EscolherNovoDestino()
    {
        if (_gameManager.slimeWayPoints.Length == 0)
        {
            Debug.LogError("Nenhum waypoint disponível!");
            return;
        }
        int idAnterior = idWayPoint;

        // Garante que o novo waypoint seja diferente do anterior
        do
        {
            idWayPoint = Random.Range(0, _gameManager.slimeWayPoints.Length);
        } while (idWayPoint == idAnterior && _gameManager.slimeWayPoints.Length > 1);

        destination = _gameManager.slimeWayPoints[idWayPoint].position;
        agent.SetDestination(destination);
    }

    //--------------------------ALERT
    IEnumerator ALERT()
    {
        yield return new WaitForSeconds(_gameManager.slimeAlertTime);
        isRunningCoroutine = true;

        if(isPlayerVisible == true)
        {
            ChangeState(enemyState.FOLLOW);//Começa a patrulhar quando ve o inimigo
        }
        else
        {
            TrocarEstado(10);//10% de chance para mudar de estado
        }
    }

    //-------------------------Ataque
    IEnumerator ATACK()
    {
        yield return new WaitForSeconds(1);
        isAtack = false;
    }


    private void OnTriggerEnter(Collider other)
    {
        isPlayerVisible = true;

        if (other.gameObject.CompareTag("Player") && (state == enemyState.IDLE || state == enemyState.PATROL))
        {

            ChangeState(enemyState.ALERT);
            m_Animator.SetBool("isAlert", true);//Alerta animação
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            isPlayerVisible = false;
            m_Animator.SetBool("isAlert", false);//Alerta

        }
    }


    //-----------------------------IA
    //Seleciona o Estado do inimigo e executa a função de cada estado
    void StateManager()
    {
        if (state == enemyState.IDLE && isRunningCoroutine == true)
        {
            agent.stoppingDistance = 0;
            agent.SetDestination(transform.position);
            isRunningCoroutine = false;
            m_Animator.SetBool("isWalk", false);//Movimentação
            m_Animator.SetBool("isAlert", false);//Movimentação
            print(state);
        }

        else if (state == enemyState.PATROL)
        {//Caso por algum erro inimigo não chegou ao ponto final ele reinicia a movimentação
            if (agent.remainingDistance <= 0.2f || !agent.hasPath)
            {
                agent.stoppingDistance = 0f;//Zera a distancia para chegar nos pontos corretos
                EscolherNovoDestino();//Realiza a movimentação do estado PATROL
                m_Animator.SetBool("isWalk", true);//Movimentação
                m_Animator.SetBool("isAlert", false);//Alerta animação
                print(state);
            }
        }

        else if (state == enemyState.FURY)
        {
            destination = _gameManager.player.position;//Distancia do game manager de 2.3 
            agent.destination = destination;
            agent.stoppingDistance = _gameManager.slimeDistanceAtack;//Distancia do ataque
            m_Animator.SetBool("isWalk", true);//Movimentação
            print(state);
            LookAt();

            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                AtaqueFury();
            }
        }//Seguindo quando player atacar inimigo

        else if (state == enemyState.ALERT)
        {
            agent.stoppingDistance = 0f;//Zera a distancia para chegar nos pontos corretos
            destination = transform.position;
            agent.destination = destination;
            m_Animator.SetBool("isWalk", false);//Movimentação
            print(state);
            LookAt();
        }

        else if (state == enemyState.FOLLOW)//Seguindo quando ve o inimigo
        {
            destination = _gameManager.player.position;
            agent.destination = destination;
            m_Animator.SetBool("isWalk", true);//Movimentação
            agent.stoppingDistance = _gameManager.slimeDistanceAtack;//Distancia do ataque
            print(state);
            LookAt();

            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                Attack();
            }
        }
    }

    //Muda o estado do inimigo -> Usando a Coroutine pra ser realizado em um tempo determinado
    //    O jogo chama ChangeState(enemyState newState), que muda o estado e, se necessário, inicia uma Coroutine.
    //O StateManager() é chamado continuamente e executa a lógica do estado atual.
    //Se necessário, ele ajusta o destino do inimigo, animações e ataques.
    void ChangeState(enemyState newState)
    {
        state = newState;
        StopAllCoroutines();

        switch (state)
        {
            case enemyState.IDLE:
                StartCoroutine(IDLE());//Rotina PARADO
                break;

            case enemyState.PATROL:
                StartCoroutine(PATROL());//ROTINA DE PATRULHA
                break;

            case enemyState.FURY:
                //Não tem necessidade da rotina UPDATE ja faz
                break;

            case enemyState.ALERT:
                StartCoroutine(ALERT());
                break;

            case enemyState.FOLLOW:
                //Não tem necessidade da rotina  UPDATE ja faz
                break;

            //Caso nenhuma rotina for iniciada
            default:
                state = enemyState.IDLE;
                StartCoroutine(IDLE());
                break;
        }
    }

    //Sorteio da Rotinas
    void TrocarEstado(int chance)
    {
        if (Random.Range(0, 100) <= chance)
            ChangeState(enemyState.IDLE);
        else
            ChangeState(enemyState.PATROL);
    }

    //---------------------------------------Ataque
    void Attack()
    {
        if (isAtack == false && isPlayerVisible == true)
        {
            isAtack = true;
            m_Animator.SetTrigger("AttackTrigger");
        }
    }
    private void AtackIsDone()
    {
        StartCoroutine(ATACK());
    }

    //-------------------------------------Olhar para o player
    private void LookAt()
    {
        //Pegar diteção
        Vector3 loockDirection = (_gameManager.player.position - transform.position).normalized;

        //Rotação
        Quaternion lookRotation = Quaternion.LookRotation(loockDirection);

        //Interpolação esférica definir rotação cálculo suavizado
        transform.rotation = Quaternion.Slerp(transform.rotation,lookRotation,_gameManager.slimeLookAtSpeed * Time.deltaTime);

        m_Animator.SetBool("isAlert", true);//Alerta animação
    }

    //------------------------------Fury 
    private void AtaqueFury()
    {
        atackDelay -= Time.deltaTime;

        if (atackDelay <= 0 )
        {
            m_Animator.SetTrigger("AttackTrigger");
            atackDelay = 2;
        }
    }

}
