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
        
        point.Add((GameObject)Instantiate(pObj, new Vector3(0, 3f, 0.0f), Quaternion.identity));
        point.Add((GameObject)Instantiate(pObj, new Vector3(0, 2.1f, 0.0f), Quaternion.identity));
        point.Add((GameObject)Instantiate(pObj, new Vector3(0, 2.5f, 0.0f), Quaternion.identity));
        point.Add((GameObject)Instantiate(pObj, new Vector3(0, 2.7f, 0.0f), Quaternion.identity));
        point.Add((GameObject)Instantiate(pObj, new Vector3(0, 1.2f, 0.0f), Quaternion.identity));
        point.Add((GameObject)Instantiate(pObj, new Vector3(0, 0.5f, 0.0f), Quaternion.identity));
        point.Add((GameObject)Instantiate(pObj, new Vector3(0, 0.1f, 0.0f), Quaternion.identity));
        point.Add((GameObject)Instantiate(pObj, new Vector3(0, 2.5f, 0.0f), Quaternion.identity));
        point.Add((GameObject)Instantiate(pObj, new Vector3(1, 4f, 0.0f), Quaternion.identity));
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

//          pos = SlowConvexHull(pos);
            pos = ConvexHull(pos);
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

    void printAry(List<Vector3> argPoint){
        string t;
        t = "";
        foreach(var v in argPoint) t += ", " + v;
        Debug.Log(t);
    }

    List<Vector3> ConvexHull(List<Vector3> argPoint){

        List<Vector3>   lupperList    = new List<Vector3>();
        List<Vector3>   llowerList    = new List<Vector3>();
        int ite;
        int listnum;
        int pn = argPoint.Count;

        if(argPoint.Count < 3) return argPoint;

        //  1. 点をx座標値の順にソートし、得られたソート列をp1...pnとする
        QuickSort(argPoint, 0, pn - 1);
        printAry(argPoint);

        //  2. リストLupperを(p1, p2)と初期設定する
        lupperList.Add(argPoint[0]);
        lupperList.Add(argPoint[1]);
        listnum = 1;

        for(ite = 2; ite < pn; ite++){ //  3. for i <- 3 to n
            //  4. do pi を Lupperに追加する
            lupperList.Add(argPoint[ite]);
            listnum++;

            //  5. while Lupperが3点以上を含んでいて、しかもLupperの最後の3点が右回りでない
            while((lupperList.Count >= 3) && (side(lupperList[listnum - 2], lupperList[listnum - 1], lupperList[listnum]) < 0)){
                lupperList.RemoveAt(listnum - 1); //  6. do 最後の3点のうち中央の点をリストLupperから削除
                listnum--;
            }
        }

        //  7. リストLlowerを(pn, pn-1)と初期設定する
        llowerList.Add(argPoint[pn - 1]);
        llowerList.Add(argPoint[pn - 2]);
        listnum = 1;

        for(ite = pn - 3; ite >= 0; ite--){ //  8. for i <- n - 2 downto 1
            //  9. do piをLlowerに追加する
            llowerList.Add(argPoint[ite]);
            listnum++;

            // 10. while Lupperが3点以上を含んでいて、しかもLlowerの最後の3点が右回りでない
            while((llowerList.Count >= 3) && (side(llowerList[listnum - 2], llowerList[listnum - 1], llowerList[listnum]) < 0)){
                llowerList.RemoveAt(listnum - 1); // 11. do 最後の3点のうちの中央の点をリストLlowerから削除
                listnum--;
            }
            
        }

        // 12. 上部と下部の凸包は最初の点と最後の点が同一であるので、その重複を避けるためにLlowerからそれらの2点を削除する。
        llowerList.RemoveAt(0);
        //llowerList.RemoveAt(llowerList.Count - 1); // 線をつなぐために最後に最初の点を追加する必要あり

        // 13. LlowerとLupperを連結し、得られたリストをLと呼ぶ
        llowerList.ForEach(llowerList => lupperList.Add(llowerList));
       
        return lupperList; // 14. return L
    }

    // 引数で渡された値の中から中央値を返す
    float GetMediaumValue(float top, float mid, float bottom){
        if(top < mid){
            if(mid < bottom){
                return mid;
            }else if(bottom < top){
                return top;
            }else{
                return bottom;
            }
        }else{
            if(bottom < mid){
                return mid;
            }else if(top < bottom){
                return top;
            }else{
                return bottom;
            }
        }
    }

    

    void QuickSort(List<Vector3> argData, int left, int right){

        Vector3 tmp;

        if(left >= right){
            return;
        }

        int i = left;  // ->方向のイテレータ
        int j = right; // <-方向のイテレータ
        float pivot = GetMediaumValue(argData[i].x, argData[(left + right) / 2].x, argData[j].x); // 中央値をピボットとする

        while(true){
            // 交換対象のインデックスを検索
            while(argData[i].x < pivot){ i++; }
            while(argData[j].x > pivot){ j--; }
            if(i >= j)break; // イテレータが反転したらbreak

            // swap
            tmp = argData[i];
            argData[i] = argData[j];
            argData[j] = tmp;

            i++;
            j--;
        }

        QuickSort(argData, left , i - 1);
        QuickSort(argData, j + 1, right);
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
