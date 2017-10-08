﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _netBrush : MonoBehaviour
{
    
    public Transform pos;
    public Renderer r;

    public int hCellNum;
    public int vCellNum;
    public float cellSize;
    public int ammo;
    public int playerNum;
    public Color myColor;
    public bool isPainting;
    public bool onBucket;
    public bool onGrid;
    public string drawType;
    public int maxAmmo;
    public int paintTypeThreshold;
    public Vector3 lookAhead;
    public Vector3 start;
    public Vector3 tempLerp;
    public float ACR; //ammo consumption rate
    public float lookAheadDist;
    public float dotSize;
    public float lineSize;
    public bool waitingFrame; //make sure the brush is active for at least 1 frame
    public GameObject[,] cells;
    public Vector2[,] celLox;
    public _netCell[,] gotCell;
    public Vector3 bc;
    public int iOfCel;
    public int jOfCel;
    public bool pullback1;
    public bool pullback2;
    public Vector2 tlerp;
    public bool used; //doc
    public bool lastFramePainting;


    // Use this for initialization
    void Start()
    {
        hCellNum = 25;
        vCellNum = 25;
        cells = new GameObject[hCellNum, vCellNum];
        cellSize = .2f;
        celLox = new Vector2[hCellNum, vCellNum];
        gotCell = new _netCell[hCellNum, vCellNum];
        pos = GetComponent<Transform>();
        r = GetComponent<Renderer>();
        r.material.color = Color.cyan;
        //myColor = Color.blue;
        ammo = 0;
        playerNum = 0;
        onBucket = false;
        //onGrid = false; 
        maxAmmo = 360;
        paintTypeThreshold = 180;
        ACR = 300;
        dotSize = .4f;
        lineSize = .1f;
        used = false;

        GetComponent<Renderer>().sortingLayerName = "fg";
        GetComponent<Renderer>().sortingOrder = 0;
    }

    // Update is called once per frame
    void Update()
    {
        /*
		if (onBucket && isPainting)
        {
            if (ammo <maxAmmo)
            {
                //ammo++;
                ammo = ammo + 3;
                if (ammo==maxAmmo)
                {
                    Debug.Log("full charge");
                }
                if (ammo==paintTypeThreshold)
                {
                    Debug.Log("half charge");
                }
            }

        }
        */
        //deprocate soon
        /*
        if (isPainting && !onBucket)
        {
            if (drawType == "dot")
            {

                if (!waitingFrame)
                {
                    //ammo = 0;
                    //dotPaint();
                }
                waitingFrame = false;
            } 
            if (drawType == "line")
            {
                if (!waitingFrame)
                {
                    //bcLerpPaint();
                }
                waitingFrame = false;
            }
        }
        */

    }

    public void moveToXY(float x, float y)
    {
        pos.position = new Vector3(x, y, 0);
    }

    public void moveToXYcapped(float x, float y)
    {
        lookAhead = new Vector3(x, y, 0);
        bc = pos.position;
        start = pos.position;
        //Debug.Log("bc" + bc);

        lookAheadDist = Vector3.Distance(lookAhead, start);
        if (lookAheadDist * ACR > ammo)
        {
            Debug.Log("last frame drawing");
            lastFramePainting = true;
            lookAhead = Vector3.Lerp(start, lookAhead, ammo / (lookAheadDist * ACR));
            pos.position = lookAhead;
            /*
            for (int t=0; t<10; t++)
            {
                tempLerp = Vector3.Lerp(start,lookAhead,t/10);
                pos.position = tempLerp;
            }
            */
            ammo = 0;
            drawType = "";
        }
        else
        {
            //Debug.Log("drawing");
            pos.position = lookAhead;
            /*
            for (int t = 0; t < 10; t++)
            {
                tempLerp = Vector3.Lerp(start, lookAhead, t / 10);
                pos.position = tempLerp;
            }
            */
            ammo = Mathf.RoundToInt(ammo - (Mathf.Max(lookAheadDist, .01f) * ACR));
            if (ammo < 0)
            {
                ammo = 0;
                drawType = "";
                lastFramePainting = true;
            }
        }

        //Debug.Log("pos" + pos.position);
        used = true;
        bcLerpPaint();

    }

    public void dotPaint()
    {
        cellPullback(pos.position.x, pos.position.y);
        paintBlob(iOfCel, jOfCel, dotSize / 2);
    }

    public void bcLerpPaint()
    {
        for (int t = 0; t < 10; t++)
        {
            //Debug.Log(t / 10.00f);
            tlerp = Vector2.Lerp(lookAhead, bc, t / 10.00f);
            cellPullback(tlerp.x, tlerp.y);
            //Debug.Log(t+": "+tlerp);
            paintBlob(iOfCel, jOfCel, lineSize / 1.5f);
        }
        //Debug.Log("tlerp bc" + bc);
        //Debug.Log("lerp lookahead" + lookAhead);
    }

    public void paintBlob(int i0, int j0, float r)
    {
        /*
        for(int i = 0; i < hCellNum; i++)
        {
            for (int j = 0; j < vCellNum; j++)
            {
                if ( Vector2.Distance(celLox[i,j],celLox[i0,j0])<r)
                {
                    gotCell[i,j].color(playerNum, myColor);
                }
            }
        }
        */
        int t = 0;
        int i;
        int j;
        while (gotCell[i0, j0].dists[t] < r * (1 / cellSize))
        {
            i = (int)gotCell[i0, j0].bfs[t].x;
            j = (int)gotCell[i0, j0].bfs[t].y;
            gotCell[i, j].color(playerNum, myColor);
            t++;
        }
    }

    public void cellPullback(float x, float y)
    {
        pullback1 = true;
        pullback2 = true;

        for (int i = 0; i < hCellNum; i++)
        {
            for (int j = 0; j < vCellNum; j++)
            {
                if (pullback1)
                {
                    if (celLox[i, j].x > x)
                    {
                        if ((celLox[i, j].x - x > (cellSize / 2)) && i > 0)
                        {
                            iOfCel = i - 1;
                            pullback1 = false;
                        }
                        else
                        {
                            iOfCel = i;
                            pullback1 = false;
                        }

                    }
                }
                if (pullback2)
                {
                    if (celLox[i, j].y < y)
                    {
                        if ((y - celLox[i, j].y > (cellSize / 2)) && j > 0)
                        {
                            jOfCel = j - 1;
                            pullback2 = false;
                        }
                        else
                        {
                            jOfCel = j;
                            pullback2 = false;
                        }
                    }
                }
            }
        }
    }

    public void startPainting()
    {
        if (ammo > 0 || onBucket)
        {
            r.material.color = Color.blue;
            isPainting = true;
        }

    }

    public void stopPainting()
    {
        r.material.color = Color.cyan;
        isPainting = false;
    }

    public void OnTriggerEnter(Collider col)
    {
        if (col.tag == "bucket")
        {
            if (col.GetComponent<_netBucket>().playerNum == playerNum)
            {
                onBucket = true;
            }
        }

        //deprocate soon
        /*
        if (col.tag == "cell")
        {
            if (isPainting)
            {
                col.GetComponent<cell>().color(playerNum, myColor);
            }
        }
        */
    }

    //deprocate soon
    public void OnTriggerStay(Collider col)
    {
        //Debug.Log("in something");
        /*
        if (col.tag == "cell")
        {
            //Debug.Log("in a cell");
            if (isPainting)
            {
                col.GetComponent<cell>().color(playerNum, myColor);
                //Debug.Log("coloring a cell");
            }
        }
        */
    }


    public void OnTriggerExit(Collider col)
    {
        if (col.tag == "bucket")
        {
            //Debug.Log("buckets");
            if (col.GetComponent<_netBucket>().playerNum == playerNum)
            {
                onBucket = false;
                if (ammo < maxAmmo && ammo > paintTypeThreshold)
                {
                    ammo = paintTypeThreshold;
                    drawType = "dot";
                    pos.localScale = new Vector3(dotSize, dotSize, dotSize);
                    waitingFrame = true;
                }
                if (ammo >= maxAmmo)
                {
                    drawType = "line";
                    pos.localScale = new Vector3(lineSize, lineSize, lineSize);
                    waitingFrame = true;
                    ammo = maxAmmo;
                    used = false;
                }
                if (ammo < paintTypeThreshold)
                {
                    ammo = 0;
                    drawType = "";
                    pos.localScale = new Vector3(dotSize, dotSize, dotSize);
                    waitingFrame = true;
                }
            }
        }

    }


}