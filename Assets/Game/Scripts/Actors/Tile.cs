#region _____________________________/ INFOS
//  AUTHOR : Nathan THEOPHILE (2025)
//  Engine : Unity
//  Note : MY_CONST, myPublic, m_MyProtected, _MyPrivate, lMyLocal, MyFunc(), pMyParam, onMyEvent, OnMyCallback, MyStruct
#endregion

using UnityEngine;

namespace Rush.Game
{
    public class Tile : MonoBehaviour
    {
        [SerializeField] public TileVariants tileVariant = TileVariants.Default;
        protected Vector3 m_Direction;
        public Vector3 direction { get => m_Direction; }

        public enum TileOrientations { Right, Left, Up, Down }

        public enum TileVariants
        {
            Default,
            Arrow,
            Convoyer,
            Dispatcher,
            Teleporter,
            Stopper,
            Spawner,
            Target,
        }

        protected virtual void Start() { m_Direction = transform.forward; }
    }
}