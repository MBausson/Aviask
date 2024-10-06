using AviaskApi.Entities;
using AviaskApi.Services.MockExaminer;
using Microsoft.Extensions.Logging;

namespace AviaskApiTest.Specs.Jobs;

public class MockExamTimerJobTest
{
    private readonly MockExamTimerJob _job;
    private readonly Mock<MockExamRepository> _mockExamRepository;

    public MockExamTimerJobTest()
    {
        _mockExamRepository = new Mock<MockExamRepository>(null);

        _job = new MockExamTimerJob(_mockExamRepository.Object, Mock.Of<ILogger<MockExamTimerJob>>());
    }

    [Fact]
    public async Task FinishMockExamAsync_QuestionNotFound()
    {
        _mockExamRepository.Setup(m => m.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((MockExam?)null);

        //  No error should be thrown
        await _job.FinishMockExamAsync(new Guid());
    }

    [Fact]
    public async Task FinishMockExamAsync_AlreadyFinished()
    {
        var mockExam = new MockExamFactory().GetOne();
        mockExam.Status = MockExamStatus.FINISHED;

        _mockExamRepository.Setup(m => m.GetByIdAsync(mockExam.Id)).ReturnsAsync(mockExam);
        _mockExamRepository.Setup(m => m.FinishMockExamAsync(It.IsAny<MockExam>())).Verifiable();

        //  No error should be thrown
        await _job.FinishMockExamAsync(mockExam.Id);

        _mockExamRepository.Verify(m => m.FinishMockExamAsync(It.IsAny<MockExam>()), Times.Never);
    }

    [Fact]
    public async Task FinishMockExamAsync_FinishedMockExam()
    {
        var mockExam = new MockExamFactory().GetOne();
        mockExam.Status = MockExamStatus.ONGOING;

        _mockExamRepository.Setup(m => m.GetByIdAsync(mockExam.Id)).ReturnsAsync(mockExam);
        _mockExamRepository.Setup(m => m.FinishMockExamAsync(It.IsAny<MockExam>())).Verifiable();

        //  No error should be thrown
        await _job.FinishMockExamAsync(mockExam.Id);

        _mockExamRepository.Verify(m => m.FinishMockExamAsync(It.IsAny<MockExam>()), Times.Once);
    }
}