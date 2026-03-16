using System;

namespace BetterCommentsPlus.Options
{
    public class StyleRule : PropertyChangeNotifier
    {
        private string id;
        private int order;
        private bool isActive;
        private bool isPredefined;
        private string criteria;
        private ForegroundStyle foreground;
        private BackgroundStyle background;

        public string Id
        {
            get { return id; }
            set { SetField(ref id, value); }
        }

        public int Order
        {
            get { return order; }
            set { SetField(ref order, value); }
        }

        public bool IsActive
        {
            get { return isActive; }
            set { SetField(ref isActive, value); }
        }

        public bool IsPredefined
        {
            get { return isPredefined; }
            set { SetField(ref isPredefined, value); }
        }

        public string Criteria
        {
            get { return criteria; }
            set { SetField(ref criteria, value); }
        }

        public ForegroundStyle Foreground
        {
            get { return foreground; }
            set { SetField(ref foreground, value); }
        }

        public BackgroundStyle Background
        {
            get { return background; }
            set { SetField(ref background, value); }
        }

        public StyleRule()
        {
            id = Guid.NewGuid().ToString();
            order = 0;
            isActive = true;
            isPredefined = false;
            criteria = string.Empty;
            foreground = new ForegroundStyle();
            background = new BackgroundStyle();
        }
    }
}
