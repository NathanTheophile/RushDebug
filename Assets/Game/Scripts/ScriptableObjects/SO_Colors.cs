#region _____________________________/ INFOS
//  AUTHOR : Nathan THEOPHILE (2025)
//  Engine : Unity
//  Note : MY_CONST, myPublic, m_MyProtected, _MyPrivate, lMyLocal, MyFunc(), pMyParam, onMyEvent, OnMyCallback, MyStruct
#endregion

using UnityEngine;

namespace Rush.Game
{
    [CreateAssetMenu(fileName = "SO_Colors", menuName = "Scriptable Objects/Colors")]
    public class SO_Colors : ScriptableObject
    {
        public enum ColorsData { Blue, Green, Orange, Yellow, Red, Purple, White }
        [SerializeField] public ColorsData ColorData;
        [SerializeField] private Color _Color;
        [SerializeField] private Material _Material;
        [SerializeField] private Material _Emissive;
        [SerializeField] private Material _CubeMaterial;


        public Color Color { get { return _Color; } set { _Color = value; } }
        public Material Material { get { return _Material; } set { _Material = value; } }
        public Material Emissive { get { return _Emissive; } set { _Emissive = value; } }
        public Material CubeMaterial { get { return _CubeMaterial; } set { _CubeMaterial = value; } }       
    }
}
