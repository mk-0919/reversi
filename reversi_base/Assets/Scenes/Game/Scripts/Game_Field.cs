using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 盤面の制御クラス
/// </summary>

public class Game_Field : UIBehaviour
{
    /// <summary>
    /// 石の色定義
    /// </summary>
    public enum StoneColor
    {
      None,
      Black,
      White
    }

    [SerializeField]
    Game_Cell cellPrefab;

    List<Game_Cell> cells = new List<Game_Cell>();
    int possibleCoroutineCount;

    protected override void Awake()
    {
        base.Awake();
        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                Game_Cell cell = Instantiate(cellPrefab);
                cell.transform.SetParent(transform);
                cell.transform.localScale = Vector3.one;
                cell.Initialize(x, y);
                cells.Add(cell);
            }
        }
    }

    /// <summary>
    /// フィールド初期化処理
    /// </summary>
    public void Initialize()
    {
        foreach (Game_Cell cell in cells)
        {
            // 全てのマスを空に
            cell.SetStoneColor(StoneColor.None);
            // 中央の4マスに初期石を配置
            if (cell.X == 3)
            {
                if (cell.Y == 3)
                {
                    cell.SetStoneColor(StoneColor.White);
                }
                else if(cell.Y == 4)
                {
                    cell.SetStoneColor(StoneColor.Black);
                }
            }
            else if (cell.X == 4)
            {
                if (cell.Y == 3)
                {
                    cell.SetStoneColor(StoneColor.Black);
                }
                else if (cell.Y == 4)
                {
                    cell.SetStoneColor(StoneColor.White);
                }
            }
        }
       
    }

    /// <summary>
    /// 全てのマスをクリック不可にします
    /// </summary>
    public void Lock()
    {
       foreach (Game_Cell cell in cells)
        {
            cell.SetClickable(false);
        }
    }

    /// <summary>
    /// 指定したマスを起点に、可能であれば石をひっくり返します
    /// </summary>
  
    public void TurnOverStoneIfPossible(Game_Cell cell)
    {
        StartCoroutine(CallTurnStoneCoroutine(cell));
    }

    /// <summary>
    /// 指定色の石の数を数えます
    /// </summary>
    
    public int CountStone(StoneColor stoneColor)
    {
        int returnCount = 0;

        foreach(Game_Cell cell in cells)
        {
            if(cell.GetStoneColor() == stoneColor)
            {
                returnCount++;
            }
        }

        return returnCount;
    }

    /// <summary>
    /// クリック可能なマスの数を数えます
    /// </summary>
    public int CountClickableCells()
    {
        int returnCount = 0;

        foreach(Game_Cell cell in cells)
        {
            if(cell.GetClickable() == true)
            {
                returnCount++;
            }
        }

        return returnCount;
    }

    /// <summary>
    /// 各マスのクリック可否状態を更新します
    /// </summary>
    public void UpdateCellsClickable(StoneColor stoneColor)
    {
       foreach(Game_Cell cell in cells)
        {
            cell.SetClickable(IsStonePuttableCell(cell, stoneColor));
        }
    }

    /// <summary>
    /// 石が配置可能なマスかどうか確認します
    /// </summary>
   
    bool IsStonePuttableCell(Game_Cell cell, StoneColor stoneColor)
    {
        if (cell.GetStoneColor() == StoneColor.None)
        {
            return Turncheck(cell, stoneColor, -1, -1) ||
                Turncheck(cell, stoneColor, -1, 0) ||
                Turncheck(cell, stoneColor, -1, 1) ||
                Turncheck(cell, stoneColor, 0, -1) ||
                Turncheck(cell, stoneColor, 0, 1) ||
                Turncheck(cell, stoneColor, 1, -1) ||
                Turncheck(cell, stoneColor, 1, 0) ||
                Turncheck(cell, stoneColor, 1, 1)
                ;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 任意のマスから指定された方向に対し、相手の石を挟める場所に自分の石が配置されているかチェックします
    /// </summary>
    
    bool Turncheck(Game_Cell cell, StoneColor stoneColor, int xDirection, int yDirection)
    {
        int x = cell.X;
        int y = cell.Y;
        //相手の石を挟めるかどうか
        bool existEnemyStone = false;
        while (true)
        {
            //各座標をそれぞれのDirectionずつずらす
            x = x + xDirection;
            y = y * yDirection;
            //(x,y)座標のcellを取得
            Game_Cell targetCell = GetCell(x, y);
            //targetCellがnullまたは、色が無い場合、false
            if (targetCell == null || targetCell.GetStoneColor() == StoneColor.None)
            {
                return false;
            }
            //targetCellと任意のマスの色が同じだった場合、その時点でのexistEnemyStoneを返す
            else if (targetCell.GetStoneColor() == stoneColor)
            {
                return existEnemyStone;
            }
            //targetCellと任意のマスの色が違う場合、existEnemyStoneをtrueに
            else
            {
                existEnemyStone = true;
            }
        }
    }

    /// <summary>
    /// 石をひっくり返すコルーチンを呼ぶためのコルーチンです。
    /// </summary>
    
    IEnumerator CallTurnStoneCoroutine(Game_Cell cell)
    {
        //誤操作防止のため全マスをクリック不可にする
        foreach(Game_Cell c in cells)
        {
            c.SetClickable(false);
        }
        //8方向それぞれに対し、ひっくり返す処理を実行
        possibleCoroutineCount = 8;
        StartCoroutine(TurnStoneCoroutine(cell, -1, -1));
        StartCoroutine(TurnStoneCoroutine(cell, -1, 0));
        StartCoroutine(TurnStoneCoroutine(cell, -1, 1));
        StartCoroutine(TurnStoneCoroutine(cell, 0, -1));
        StartCoroutine(TurnStoneCoroutine(cell, 0, 1));
        StartCoroutine(TurnStoneCoroutine(cell, 1, -1));
        StartCoroutine(TurnStoneCoroutine(cell, 1, 0));
        StartCoroutine(TurnStoneCoroutine(cell, 1, 1));
        while(possibleCoroutineCount > 0)
        {
            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitForSeconds(0.5f);
        Game_SceneController.Instance.OnTurnStoneFinished();
    }

    /// <summary>
    /// 石をひっくり返すコルーチンです。（指定した1方向に対して処理をかける実働メソッド）
    /// </summary>
    
    IEnumerator TurnStoneCoroutine(Game_Cell cell, int xDirection, int yDirection)
    {
        //Tuencheckメソッドにより挟めるかどうか判定、挟める場合のみ続行
        if (!Turncheck(cell, cell.GetStoneColor(), xDirection, yDirection))
        {
            possibleCoroutineCount--;
            yield break;
        }

        int x = cell.X;
        int y = cell.Y;

        while (true)
        {
            x += xDirection;
            y += yDirection;
            Game_Cell targetCell = GetCell(x, y);
            //targetCellがnullもしくは任意のセルと同じ色になった場合、処理を終了
            if(null == targetCell || targetCell.GetStoneColor() == cell.GetStoneColor())
            {
                possibleCoroutineCount--;
                break;
            }
            else
            //相手セルの色を任意のセルと同じ色に変更
            {
                yield return new WaitForSeconds(0.5f);
                targetCell.SetStoneColor(cell.GetStoneColor());
                Game_SoundManager.Instance.turn.Play();
            }
        }
    }

    /// <summary>
    /// 指定場所のマスを取得します
    /// </summary>
  
    Game_Cell GetCell(int x, int y)
    {
        Game_Cell target_Cell = new Game_Cell();
        foreach (Game_Cell cell in cells)
        {
            if (cell.X == x && cell.Y == y)
            {
                target_Cell = cell;
            }
        }
        return target_Cell;
    }
}
