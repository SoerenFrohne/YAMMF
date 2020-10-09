namespace Core.YAMMF.TimeSeriesModel
{
 /**
 * Conditioning Features are composed of the items that are each sampled along the
 * past-to-current time series window (-1 second to 0/now)
 */
 public class ConditioningFeatures
 {
  /*
     – Ball Movement B: We use the past movement of the ball of positions P ∈ R^3T and velocities V ∈ R^3T to guide 
     the prediction for next frame. This conditioning is helpful since ball movement
    is typically highly fast-paced. We further give the ball control
    weights W ∈ R^T as input to the network, which were used to
    transform the original ball parameters from Bi to ˆBi in order to
    learn the ball movement only within a control radius around the
    character (see Appendix A.2).
    
    – Contact Information C: Similarly, we condition the generated
    motion on the contacts Ci ∈ R^5T for feet, hands and ball that
    appeared during the past to stabilize the movements (see Fig. 3, left bottom).
     */
 }
}