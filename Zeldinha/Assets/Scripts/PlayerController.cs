using System.ComponentModel;
using UnityEngine; // Importa a biblioteca principal do Unity

public class PlayerController : MonoBehaviour
{

    [Header("Config Player")] // Exibe um título no Inspector para organizar variáveis no Unity
    private CharacterController controller; // Referência ao componente CharacterController, que gerencia o movimento do jogador
    [SerializeField] float speedMoviment = 3f; // Velocidade de movimento do jogador (editável no Inspector)
    private Vector3 direction; // Vetor para armazenar a direção do movimento (eixo Z -> frente e trás)
    public int hp = 3;
    private bool isDie;


    [Header("Animação")]
    private Animator anim;
    private bool isWalk;

    [Header("Atack")]
    [SerializeField] ParticleSystem fxAtack;
    [SerializeField]private bool isAtack;

    [Header("Área de Atack")]
    public Transform hitBox;
    [Range(0.2f, 1f)]//Barra deslizante
    public float hitRange = 0.5f;
    [SerializeField] Collider[] hitInfo;//Lista de colisões
    [SerializeField] LayerMask hitMask;//Oque pode ser atingido
    public int amountDmg;//quantidade de dano


    //Atividade do Player
    private GameManager _gameManager;

    // Start é chamado uma vez antes da primeira execução do Update
    void Start()
    {
        // Obtém o componente CharacterController do objeto ao qual o script está anexado
        controller = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();//Procura o componete no filho no caso o Animator

        _gameManager = FindFirstObjectByType(typeof(GameManager))as GameManager;
    }

    // Update é chamado uma vez por frame
    void Update()
    {
        if(_gameManager.gameState != GameState.GAMEPLAY) { return; }

        // A função de movimentação deveria ser chamada aqui para que o jogador se mova
        movimentacaoPlayer();

        ataquePlayer();
    }

    // Método responsável pela movimentação do jogador
    private void movimentacaoPlayer()
    {
        // Captura a entrada do jogador nos eixos X (horizontal) e Z (vertical) do teclado
        float horizontal = Input.GetAxis("Horizontal"); // Configuração padrão do Unity (A/D ou setas esquerda/direita)
        float vertical = Input.GetAxis("Vertical"); // Configuração padrão do Unity (W/S ou setas cima/baixo)

        // Define a direção do movimento com base nas entradas do jogador
        direction = new Vector3(horizontal, 0f, vertical);

        if (direction.magnitude > 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

            // Suaviza a rotação aplicando um tempo de interpolação
            Quaternion targetRotation = Quaternion.Euler(0f, targetAngle, 0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            isWalk = true;//Está Andando
        }
        else
        {
            isWalk = false; //Está parado
        }
        // Move o jogador multiplicando a direção pela velocidade e pelo tempo do frame
        controller.Move(direction * speedMoviment * Time.deltaTime);

        //Setar a Animação
        anim.SetBool("isWalk", isWalk);
    }

    //Ataque do Player
    private void ataquePlayer()
    {
        if (Input.GetButtonDown("Fire1")&& isAtack == false)
            {
            isAtack = true;
            anim.SetTrigger("triggerAtack01");
            fxAtack.Emit(1);

            //etectar colisões dentro de uma esfera invisível.
            hitInfo = Physics.OverlapSphere(hitBox.position, hitRange,hitMask);

            foreach(Collider c in hitInfo)
            {
                c.gameObject.SendMessage("GetHit", amountDmg,SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    //Permitir novo ataque
    public void AtackIsDone()
    {
        isAtack = false;
    }

    //Área do Atack
    private void OnDrawGizmosSelected()
    {
        if (hitBox != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(hitBox.position, hitRange);
        }
    }

    //Levar Dano
    public void GetHit(int amount)
    {
        if (isDie) return;

        hp = hp - amount;

        if (hp > 0)
        {
            //Leva HIT
            anim.SetTrigger("Hit");//Animação de dano
        }
        else
        {
            //Morre
            _gameManager.ChangeGameState(GameState.DIE);
           anim.SetTrigger("Die");//Animação de Morte
           isDie = true;
        }
    }
    //----------------Recebe O dano quando o collider atinge
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("TakeDamage"))
        {

            GetHit(1);
        }
    }
}
