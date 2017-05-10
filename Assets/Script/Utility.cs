using System.Collections;
using System.Collections.Generic;


public static class Utility {

	public static T[] ShuffleArray<T> (T[] array, int seed) {
		System.Random prng = new System.Random(seed);

		for(int i=0; i<array.Length-1; i++){
			int randomIndex = prng.Next(i,array.Length);
			T tampItem = array[randomIndex];
			array[randomIndex] = array[i];
			array[i] = tampItem;
		}
		return array;
	}

	public static bool TrueOrFalse (int falsePersent, int seed) {
		bool tor;
		System.Random prng = new System.Random(seed);
		int randomValue = prng.Next(0,falsePersent);
		if(randomValue == 0)
			tor = true;
		else
			tor = false;

	return tor;
	}

	public static int randomNumber (int max, int seed) {
	System.Random prng = new System.Random(seed);
	int randomValue = prng.Next(max);

	return randomValue;
	}
		public static int randomNumber (int min, int max, int seed) {
	System.Random prng = new System.Random(seed);
	int randomValue = prng.Next(min, max);

	return randomValue;
	}
}
