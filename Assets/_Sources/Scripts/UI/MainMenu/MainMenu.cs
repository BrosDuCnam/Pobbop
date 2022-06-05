using DG.Tweening;
using UnityEngine;

namespace UI
{
    public class MainMenu : MonoBehaviour
    {
        public GameObject[] subMenus;
        public Transform creditTransform;

        public enum Direction
        {
            Front, 
            Left,
            Right
        }
        private Direction _curentDirection;

        public void Quit()
        {
            Application.Quit();
        }
        
        public void CloseAllSubMenus()
        {
            CloseAllSubMenus(false);
        }
        
        public void CloseAllSubMenusAnimated()
        {
            CloseAllSubMenus(true);
        }
        
        public void CloseAllSubMenus(bool closeAnimation, GameObject exept = null)
        {
            foreach (var subMenu in subMenus)
            {
                if (subMenu.activeSelf && subMenu != exept)
                {
                    if (closeAnimation)
                        subMenu.transform.DOScale(0, 0.5f).SetEase(Ease.OutExpo)
                            .OnComplete(() => subMenu.SetActive(false));
                    else subMenu.SetActive(false);
                }
            }
        }
        
        public void OpenSubMenuAnimated(GameObject submenu)
        {
            OpenSubMenu(submenu, true, true);
        }
        
        public void OpenSubMenu(GameObject submenu)
        {
            OpenSubMenu(submenu, true, false);
        }
        
        public void OpenSubMenuNoClose(GameObject submenu)
        {
            OpenSubMenu(submenu, false, true);
        }

        
        public void OpenSubMenu(GameObject subMenu, bool closeOpenMenus, bool closeAnimation = false)
        {
            if (closeOpenMenus)
                CloseAllSubMenus(closeAnimation, subMenu);
            
            if (subMenu.activeSelf) return;
            
            subMenu.SetActive(true);
            subMenu.transform.localScale = Vector3.zero;
            subMenu.transform.DOScale(1, 0.5f).SetEase(Ease.OutExpo);
        }

        public float DirectionToAngle(Direction direction)
        {
            switch (direction)
            {
                case Direction.Front:
                    return 0;
                case Direction.Left:
                    return -90;
                case Direction.Right:
                    return 90;
                default:
                    return 0;
            }
        }
        
        public void StartCreditsAnimation()
        {
            creditTransform.localPosition = new Vector3(0, 0, 0);
            creditTransform.DOLocalMoveY(-1000, 10).onComplete = () => RotateToFront();
        }

        #region RotateTo
        
        public void RotateToLeft()
        {
            RotateTo(Direction.Left);
        }
        
        public void RotateToRight()
        {
            RotateTo(Direction.Right);
        }
        
        public void RotateToFront()
        {
            RotateTo(Direction.Front);
        }
        
        public void RotateTo(Direction direction)
        {
            transform.DOLocalRotate(new Vector3(0, DirectionToAngle(direction), 0), 0.5f).SetEase(Ease.InOutQuint);
        }
        
        public void RotateToTop()
        {
            transform.DOLocalRotate(new Vector3(-90f, 0, 0), 0.5f).SetEase(Ease.InOutQuint);
        }
        
        #endregion
        
        #region Sounds

        public void PlayButtonHover()
        {
            //TODO: Play sound
        }

        public void PlayButtonClick()
        {
            //TODO: Play sound
        }

        public void PlayButtonBack()
        {
            //TODO: Play sound
        }

        #endregion
    }
}