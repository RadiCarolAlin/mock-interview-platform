namespace InterviewPractice.Application.Common.Exceptions;

public sealed class BusinessConflictException(string message) : Exception(message);
