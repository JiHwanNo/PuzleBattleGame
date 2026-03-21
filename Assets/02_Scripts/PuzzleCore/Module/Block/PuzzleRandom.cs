using System;

namespace Puzzle.Core
{
    /// <summary>
    /// 게임 내에서 공유되는 결정론적 난수 생성 클래스입니다.
    /// 리플레이 기능을 위해 특정 시드(Seed)와 상태를 유지합니다.
    /// </summary>
    public class PuzzleRandom
    {
        private Random _random;
        private int _seed;

        public PuzzleRandom(int seed)
        {
            _seed = seed;
            _random = new Random(_seed);
        }

        /// <summary>
        /// 지정된 범위 내의 난수를 반환합니다.
        /// </summary>
        public int Next(int minValue, int maxValue)
        {
            return _random.Next(minValue, maxValue);
        }

        /// <summary>
        /// 0.0 ~ 1.0 사이의 난수를 반환합니다.
        /// </summary>
        public double NextDouble()
        {
            return _random.NextDouble();
        }

        /// <summary>
        /// 시드를 재설정하여 난수 생성기를 초기화합니다.
        /// </summary>
        public void SetSeed(int seed)
        {
            _seed = seed;
            _random = new Random(_seed);
        }
    }
}
