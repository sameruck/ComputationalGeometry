using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class manager : MonoBehaviour
{
    public  List<GameObject>  point  = new List<GameObject>();
    public  LineRenderer renderer;
    private GameObject pObj;
    private Camera     cObj;

    // Start is called before the first frame update
    void Start()
    {
        pObj = (GameObject)Resources.Load("Circle");     // 点描画用プレハブCircleをロード
        GameObject obj = GameObject.Find("Main Camera"); // カメラオブジェクトを取得
        cObj = obj.GetComponent<Camera>();               // カメラコンポーネントを取得

        // 線の設定
        renderer.SetWidth(0.1f, 0.1f); // 線の幅
    }

    // Update is called once per frame
    void Update()
    {
        bool pAddFlg = false;

        // 左ボタンクリック
        if (Input.GetMouseButtonUp(0))
        {
            Vector3 mPos = Input.mousePosition;
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mPos);

            // プレハブを元にオブジェクトを生成する
            point.Add((GameObject)Instantiate(pObj, new Vector3(worldPos.x, worldPos.y, 0.0f), Quaternion.identity));

            pAddFlg = true;
        }

        // データ追加時に凸包描画
        if(pAddFlg)
        {
            List<Vector3> pos = new List<Vector3>();

            // GameObject -> Vector3
            foreach(var wrkPnt in point) {
                pos.Add(wrkPnt.transform.position);
            }

            pos = SlowConvexHull(pos);
            if(pos.Count > 1){
                drawLine(pos);
            }
        }
    }

    List<Vector3> SlowConvexHull(List<Vector3> argPoint){

        List<Vector3>   resultList = new List<Vector3>(); // E <- 0.
        List<Vector3>   edgeList = new List<Vector3>();
        List<Vector3>   qList    = new List<Vector3>();
        List<Vector3>   rList    = new List<Vector3>();
        Vector3 pVector = new Vector3();
        Vector3 qVector = new Vector3();
        Vector3 rVector = new Vector3();
        bool            validFlg = false;
        bool            errFlg   = false;
        int             pIte, qIte, rIte;

        for( pIte = 0; pIte < argPoint.Count; pIte++ ){
            pVector = argPoint[pIte];
            qList = new List<Vector3>(argPoint);
            qList.RemoveAt(pIte);

            for( qIte = 0; qIte < qList.Count; qIte++ ){
                qVector = qList[qIte];

                if(pVector != qVector){                            // for p ≠ qであるすべての順序対(p,q) ∈ P × P

                    rList = new List<Vector3>(qList);
                    rList.RemoveAt(qIte);
                    validFlg = true;                               // valid <- true

                    for( rIte = 0; rIte < rList.Count; rIte++ ){          // for p,q のどちらとも異なるすべての点r ∈ P
                        rVector = rList[rIte];
                        if( side(rVector, pVector, qVector) < 0 ){ // do if rがpからqへの有向直線の左にある
                            validFlg = false;                      // valid <- false
                        }
                    }

                    if(validFlg){                                  // if valid then 有向辺pqをEに追加 
                        edgeList.Add(pVector);
                        edgeList.Add(qVector);
                    }
                }
            }
        }

        // 辺集合Eから時計回りの順にソートされたCH(P)の頂点集合のリストLを構成する
        if(edgeList.Count > 0){

            // 先頭となる辺を格納する
            resultList.Add(edgeList[0]);
            resultList.Add(edgeList[1]);
            rVector      = edgeList[1];
            edgeList.RemoveAt(1);
            edgeList.RemoveAt(0);

            while(edgeList.Count > 0){
                errFlg   = true;

                for( pIte = 0; pIte < edgeList.Count; pIte+=2){
                    qIte = pIte + 1;
                    if(rVector == edgeList[pIte]){
                        resultList.Add(edgeList[qIte]);
                        rVector      = edgeList[qIte];
                        edgeList.RemoveAt(qIte);
                        edgeList.RemoveAt(pIte);
                        errFlg       = false;
                    }
                }

                if(errFlg){ // 無限ループ回避
                    Debug.Log( "ERROR 01" );
                    return resultList;
                }
            }

            if(edgeList.Count == 0){ // 末尾チェック
                if(rVector == resultList[0]){
                    Debug.Log( "OK" );
                }
            }
        }

        return resultList;
    }

    void drawLine(List<Vector3> argPoint){

        int ite = 0;

        // 頂点の数
        renderer.SetVertexCount(argPoint.Count);

        // 頂点を設定
        foreach(var wrkPnt in argPoint) {
            renderer.SetPosition(ite++, wrkPnt);
        }
    }

    int side(Vector3 p, Vector3 es, Vector3 ee){

        Vector3 e1 =  p - es;
        Vector3 e2 = ee - es;
        Vector3 z  = Vector3.Cross(e1, e2);
        if      (z.z > 0) return  1;
        else if (z.z < 0) return -1;
        else              return  0;

    }
}
