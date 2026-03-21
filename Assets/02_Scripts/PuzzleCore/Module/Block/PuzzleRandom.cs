using System;

namespace Puzzle.Core
{
    /// <summary>
    /// 프로젝트의 결정론적 난수 생성을 담당하는 클래스입니다.
    /// 동일한 시드(Seed)를 사용할 경우 항상 동일한 수열을 반환하여 리플레이를 보장합니다.
    /// 유니티 기본 랜덤이 아닌 System.Random을 래핑하여 사용합니다.
    /// </summary>
    public class PuzzleRandom
    {
        /// <summary> 내부 난수 생성 객체 </summary>
        private Random _random;

        /// <summary> 사용된 시드값 </summary>
        public int Seed { get; private set; }

        /// <summary>
        /// 특정 시드값을 사용하여 난수 생성기를 초기화합니다.
        /// </summary>
        /// <param name="seed">난수 생성용 시드</param>
        public PuzzleRandom(int seed)
        {
            Seed = seed;
            _random = new Random(seed);
        }

        /// <summary>
        /// 0.0 이상 1.0 미만의 부동 소수점 난수를 반환합니다.
        /// </summary>
        /// <returns>랜덤 소수값</returns>
        public double NextDouble()
        {
            return _random.NextDouble();
        }

        /// <summary>
        /// 지정된 범위 내의 정수 난수를 반환합니다.
        /// </summary>
        /// <param name="min">최솟값 (포함)</param>
        /// <param name="max">최댓값 (미포함)</param>
        /// <returns>랜덤 정수값</returns>
        public int Next(int min, int max)
        {
            if (min >= max)
            {
                return min;
            }
            return _random.Next(min, max);
        }
    }
}
