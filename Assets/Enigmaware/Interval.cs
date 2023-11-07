[System.Serializable]
public class Interval 
{
    public Interval(float minimum, float maximum){
        min = minimum;
        max = maximum;
        if(min > max){
            min = maximum;
            max = minimum;
        }
    }
    public float min;
    public float max;
    public bool Contains(float number){
        if(number >= min && number <= max) return true;
        else return false;
    }
}