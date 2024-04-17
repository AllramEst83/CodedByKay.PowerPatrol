namespace CodedByKay.PowerPatrol.Interfaces
{
    public interface IUserPersmissionsService
    {
        Task<bool> GetPermissionsFromUser(CancellationToken cancellationToken);
    }
}
