﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GardenEelHead : GardenEelBody
{
    public GameObject body;
    public GameObject eye;
    public Sprite idleEye;
    public Sprite stunedEye;

    private int _level = 0;
    private float _lengthOfBody;
    private bool _canMove = true;
    private Transform _tail;
    private Rigidbody2D _rigidbody2D;
    private SpriteRenderer _eyeSpriteRenderer;
    private List<GardenEelBody> _bodyList;

    private const float StartSize = 1f;
    private const float StartLength = 15f;
    private const int StartCountOfBody = 10;

    private static readonly float[] LengthArray = new[] {1f, 1.9f, 3.3f, 4.3f, 5f};
    private static readonly float[] SizeArray = new[] {1f, 1.4f, 2f, 2.6f, 3.2f};
    private static readonly int[] ObjectCountArray = new[] {10, 13, 16, 16, 16};
    
    public void LevelUp(int level)
    {
        int size = _bodyList.Count - 1;
        float before = _lengthOfBody;
        
        _lengthOfBody = StartLength * LengthArray[level] * 5;

        foreach (var bodys in _bodyList)
        {
            bodys.transform.localScale = Vector2.one * StartSize * SizeArray[level];
            bodys.spaceOfBody = _lengthOfBody / (float) ObjectCountArray[level];
        }
        
        Debug.Log($"level : {level}, bodyCount : {_bodyList.Count}");
        
        AddBody(ObjectCountArray[level] - _bodyList.Count - 1, SizeArray[level]);
    }
    
    protected override void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _eyeSpriteRenderer = eye.GetComponent<SpriteRenderer>();
        
        _bodyList = new List<GardenEelBody>();
        _bodyList.Add(this);

        _lengthOfBody = StartLength;
        transform.localScale = Vector3.one * StartSize;

        AddBody(StartCountOfBody, SizeArray[_level]);
    }

    protected override void FixedUpdate()
    {
        _target = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        var velocity = _rigidbody2D.velocity;
            
        if (velocity.y < -1f)
        {
            velocity.y = -1f;
            _rigidbody2D.velocity = velocity;
        }
        
        if ((Input.GetMouseButton(0) || Input.touchCount > 0) && _canMove)
        {
            MoveToTarget(_target);
        }
    }

    private void AddBody(int n, float sizeValue)
    {
        if (n <= 0)
        {
            return;
        }
        
        int size = _bodyList.Count - 1;
        
        _bodyList[_bodyList.Count - 1].SetIsTail(false);
        
        for (int i = 0; i < n; i++)
        {
            var obj = Instantiate(body, new Vector3(0, -6.5f, 0), Quaternion.identity);
            var bodyComponent = obj.GetComponent<GardenEelBody>();
            bodyComponent.parent = _bodyList[size + i].transform;
            bodyComponent.spaceOfBody = _lengthOfBody / (size + n) + SizeArray[_level];
            _tail = obj.transform;
            _tail.localScale *= sizeValue;
            _bodyList.Add(bodyComponent);
        }

        for (int i = _bodyList.Count - 2; i >= 0; i--)
        {
            _bodyList[i].child = _bodyList[i + 1].transform;
        }

        _bodyList[_bodyList.Count - 1].SetIsTail(true);
    }

    protected override bool MoveToTarget(Vector3 target)
    {
        Vector2 tailDir = _tail.position - transform.position;

        float magnitude = tailDir.magnitude;

        if (magnitude < _lengthOfBody)
        {
            Vector2 dir = target - transform.position;
            
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            var rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            
            var rotationVector = rotation.eulerAngles + new Vector3(0, 0, -90);
            transform.rotation = Quaternion.Euler(rotationVector);
            
            transform.Translate(Vector2.up * Time.deltaTime * speed);
            return true;
        }

        StartCoroutine(StunPlayer(.5f));

        return false;
    }

    IEnumerator StunPlayer(float time)
    {
        _canMove = false;
        _eyeSpriteRenderer.sprite = stunedEye;
        Debug.Log($"{time}초 동안 스턴걸림");
        yield return new WaitForSeconds(time);
        _eyeSpriteRenderer.sprite = idleEye;
        _canMove = true;
    }
}
