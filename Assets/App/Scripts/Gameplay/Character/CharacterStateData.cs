namespace Game.Gameplay.Character
{
    public class CharacterStateData
    {
        private bool _isDodging;
        private bool _isBlocking;
        
        public bool Dodging
        {
            get => _isDodging;
            set
            {
                if (_isBlocking && value)
                    _isBlocking = false;
                
                _isDodging = value;
            }
        }
        
        public bool Blocking
        {
            get => _isBlocking;
            set
            {
                if (_isDodging && value)
                    _isDodging = false;
                
                _isBlocking = value;
            }
        }

        public void Reset()
        {
            _isBlocking = false;
            _isDodging = false;
        }
    }
}