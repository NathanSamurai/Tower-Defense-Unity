using UnityEngine;
using System.Collections;

public class DemoController : MonoBehaviour
{
    [System.Serializable]
    public class KeyTrigger
    {
        public KeyCode key;
        public string triggerName;
        public bool isAttack;
    }

	private Animator animator;

	public float walkspeed = 5;
	private float horizontal;
	private float vertical;
	private float rotationDegreePerSecond = 1000;

	public GameObject gamecam;
	public Vector2 camPosition;
    Vector2 tempCamPosition;
    Vector2 curCamPosition { get { return (tempCamPosition.magnitude == 0) ? camPosition : tempCamPosition; } }
    private bool dead;


	public GameObject[] characters;
	public int currentChar = 0;

    public GameObject[] targets;
    public float minAttackDistance;

    public UnityEngine.UI.Text nameText;
    public KeyTrigger[] keyTriggerList;
    bool bStopMoving;

    void Start()
	{
		setCharacter(0);
	}

	void FixedUpdate()
	{
		if (animator && !dead)
		{
			//walk
			horizontal = Input.GetAxis("Horizontal");
			vertical = Input.GetAxis("Vertical");

			Vector3 stickDirection = new Vector3(horizontal, 0, vertical);
			float speedOut;

			if (stickDirection.sqrMagnitude > 1) stickDirection.Normalize();

			if (!bStopMoving)
				speedOut = stickDirection.sqrMagnitude;
			else
				speedOut = 0;

			if (stickDirection != Vector3.zero && !bStopMoving)
				transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(stickDirection, Vector3.up), rotationDegreePerSecond * Time.deltaTime);
			GetComponent<Rigidbody>().velocity = transform.forward * speedOut * walkspeed + new Vector3(0, GetComponent<Rigidbody>().velocity.y, 0);

			animator.SetFloat("Speed", speedOut);
		}
	}

	void Update()
	{
		if (!dead)
		{
			// move camera
			if (gamecam)
				gamecam.transform.position = transform.position + new Vector3(0, curCamPosition.x, -curCamPosition.y);

			// attack
			if (Input.GetButtonDown("Fire1") || Input.GetButtonDown("Jump") && !bStopMoving)
			{
				bStopMoving = true;
				animator.SetTrigger("Attack");
				StartCoroutine(stopAttack(1));
                tryDamageTarget();
            }

            foreach (var item in keyTriggerList)
            {
                if (Input.GetKeyDown(item.key) && !bStopMoving)
                {
                    bStopMoving = true;
                    animator.SetTrigger(item.triggerName);
                    StartCoroutine(stopAttack(1));
                    if (item.isAttack)
                        tryDamageTarget();
                }
            }
            

            animator.SetBool("isAttacking", bStopMoving);

			//switch character

			if (Input.GetKeyDown("left"))
			{
				setCharacter(-1);
				bStopMoving = true;
				StartCoroutine(stopAttack(1f));
			}

			if (Input.GetKeyDown("right"))
			{
				setCharacter(1);
				bStopMoving = true;
				StartCoroutine(stopAttack(1f));
			}

			// death
			if (Input.GetKeyDown("m"))
				StartCoroutine(selfdestruct());
        }
	}
    GameObject target = null;
    public void tryDamageTarget()
    {
        target = null;
        float targetDistance = minAttackDistance + 1;
        foreach (var item in targets)
        {
            float itemDistance = (item.transform.position - transform.position).magnitude;
            if (itemDistance < minAttackDistance)
            {
                if (target == null) {
                    target = item;
                    targetDistance = itemDistance;
                }
                else if (itemDistance < targetDistance)
                {
                    target = item;
                    targetDistance = itemDistance;
                }
            }
        }
        if(target != null)
        {
            transform.LookAt(target.transform);
            
        }
    }
    public void DealDamage(DealDamageComponent comp)
    {
        if (target != null)
        {
            target.GetComponent<Animator>().SetTrigger("Hit");
            var hitFX = Instantiate<GameObject>(comp.hitFX);
            hitFX.transform.position = target.transform.position + new Vector3(0, target.GetComponentInChildren<SkinnedMeshRenderer>().bounds.center.y,0);
        }
    }

    public IEnumerator stopAttack(float length)
	{
		yield return new WaitForSeconds(length); 
		bStopMoving = false;
	}

    public IEnumerator selfdestruct()
    {
        animator.SetTrigger("isDead");
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        dead = true;

        yield return new WaitForSeconds(3f);
        while (true)
        {
            if (Input.anyKeyDown)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
                yield break;
            }
            else
                yield return 0;

        }
    }
    public void setCharacter(int i)
	{
		currentChar += i;

		if (currentChar > characters.Length - 1)
			currentChar = 0;
		if (currentChar < 0)
			currentChar = characters.Length - 1;

		foreach (GameObject child in characters)
		{
            if (child == characters[currentChar])
            {
                child.SetActive(true);
                if (nameText != null)
                    nameText.text = child.name;
            }
            else
            {
                child.SetActive(false);
            }
		}
		animator = GetComponentInChildren<Animator>();
    }

    public bool ContainsParam(Animator _Anim, string _ParamName)
    {
        foreach (AnimatorControllerParameter param in _Anim.parameters)
        {
            if (param.name == _ParamName) return true;
        }
        return false;
    }
    //10 13.5
    public void TweenCamera(string data)
    {
        var dataList = data.Split('|');
        if(dataList.Length == 1)
        {
            StartCoroutine(_tweenCamera(tempCamPosition, camPosition, float.Parse(dataList[0]), () => { tempCamPosition = Vector2.zero; }));
        }
        if (dataList.Length == 3)
        {
            StartCoroutine(_tweenCamera(camPosition, new Vector2(float.Parse(dataList[0]), float.Parse(dataList[1])), float.Parse(dataList[2])));
        }
    }
    IEnumerator _tweenCamera(Vector2 fromCam, Vector2 toCam, float duration, System.Action onCompleted = null)
    {
        float timer = 0;
        while(timer < duration)
        {
            timer += Time.deltaTime;
            tempCamPosition = Vector2.Lerp(fromCam, toCam, Mathf.Clamp01(timer / duration));
            yield return 0;
        }
        if (onCompleted != null) onCompleted.Invoke();
    }
}
