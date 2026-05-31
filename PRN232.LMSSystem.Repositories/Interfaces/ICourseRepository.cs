using PRN232.LMSSystem.Repositories.Entities;
using System.Linq.Expressions;

namespace PRN232.LMSSystem.Repositories.Interfaces;

public interface ICourseRepository : IGenericRepository<Course>
{
    Task<IEnumerable<CourseWithCount>> GetCoursesWithCountAsync(
        Expression<Func<Course, bool>>? filter = null,
        Func<IQueryable<Course>, IOrderedQueryable<Course>>? orderBy = null,
        int? page = null,
        int? pageSize = null,
        bool includeEnrollments = false);

    Task<CourseWithCount?> GetCourseWithCountByIdAsync(int id, bool includeEnrollments = false);
}
