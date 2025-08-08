using AutoMapper;
using PPC.DAO.Models;
using PPC.Repository.Interfaces;
using PPC.Service.Interfaces;
using PPC.Service.Mappers;
using PPC.Service.ModelRequest.CourseRequest;
using PPC.Service.ModelRequest.SurveyRequest;
using PPC.Service.ModelResponse;
using PPC.Service.ModelResponse.SurveyResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Service.Services
{
    public class QuestionService : IQuestionService
    {
        private readonly IQuestionRepository _questionRepository;
        private readonly IMapper _mapper;


        public QuestionService(IQuestionRepository questionRepository, IMapper mapper)
        {
            _questionRepository = questionRepository;
            _mapper = mapper;
        }
            
        public async Task<ServiceResponse<string>> CreateQuestionAsync(SurveyQuestionCreateRequest request)
        {
            if (string.IsNullOrEmpty(request.SurveyId) || string.IsNullOrEmpty(request.Description))
                return ServiceResponse<string>.ErrorResponse("Dữ liệu yêu cầu không hợp lệ");

            var question = request.ToCreateSurveyQuestion();
            var answers = request.Answers.ToCreateSurveyAnswers(question.Id);

            await _questionRepository.CreateQuestionWithAnswersAsync(question, answers);

            return ServiceResponse<string>.SuccessResponse("Câu hỏi cùng các câu trả lời đã được tạo thành công");
        }

        public async Task<ServiceResponse<string>> CreateQuestion1Async(QuestionCreateRequest request)
        {
            if (string.IsNullOrEmpty(request.QuizId) || string.IsNullOrEmpty(request.Description))
                return ServiceResponse<string>.ErrorResponse("Dữ liệu yêu cầu không hợp lệ");

            var question = request.ToCreateQuestion();
            var answers = request.Answers.ToCreateAnswers(question.Id);

            await _questionRepository.CreateQuestionWithAnswersAsync(question, answers);

            return ServiceResponse<string>.SuccessResponse("Câu hỏi cùng các câu trả lời đã được tạo thành công");
        }

        public async Task<ServiceResponse<PagingResponse<SurveyQuestionDto>>> GetPagingAsync(PagingSurveyQuestionRequest request)
        {
            var (items, total) = await _questionRepository.GetPagingBySurveyAsync(request.SurveyId, request.Page, request.PageSize);
            var dtoList = _mapper.Map<List<SurveyQuestionDto>>(items);
            var paging = new PagingResponse<SurveyQuestionDto>(dtoList, total, request.Page, request.PageSize);
            return ServiceResponse<PagingResponse<SurveyQuestionDto>>.SuccessResponse(paging);
        }

        public async Task<ServiceResponse<string>> UpdateAsync(string questionId, SurveyQuestionUpdateRequest request)
        {
            var question = await _questionRepository.GetQuestionWithAnswersAsync(questionId);
            if (question == null)
                return ServiceResponse<string>.ErrorResponse("Question not found.");

            question.Description = request.Description;

            var newAnswers = request.Answers.Select(a => new Answer
            {
                Id = Utils.Utils.GenerateIdModel("Answer"),
                QuestionId = question.Id,
                Text = a.Text,
                Score = a.Score,
                Tag = a.Tag,
                Status = 1
            }).ToList();

            await _questionRepository.UpdateQuestionAndAnswersAsync(question, newAnswers);
            return ServiceResponse<string>.SuccessResponse("Updated successfully.");
        }

        public async Task<ServiceResponse<string>> UpdateAsync(string questionId, QuestionUpdateRequest request)
        {
            var question = await _questionRepository.GetQuestionWithAnswersAsync(questionId);
            if (question == null)
                return ServiceResponse<string>.ErrorResponse("Không tìm thấy câu hỏi");

            question.Description = request.Description;

            var newAnswers = request.Answers.Select(a => new Answer
            {
                Id = Utils.Utils.GenerateIdModel("Answer"),
                QuestionId = question.Id,
                Text = a.Text,
                Score = a.Score,
                Status = 1
            }).ToList();

            await _questionRepository.UpdateQuestionAndAnswersAsync(question, newAnswers);
            return ServiceResponse<string>.SuccessResponse("Cập nhật thành công");
        }


        public async Task<ServiceResponse<string>> DeleteAsync(string questionId)
        {
            var result = await _questionRepository.DeleteWithAnswersAsync(questionId);
            return result
                ? ServiceResponse<string>.SuccessResponse("Xóa thành công")
                : ServiceResponse<string>.ErrorResponse("Không tìm thấy câu hỏi");
        }

        public async Task<ServiceResponse<List<SurveyQuestionDto>>> GetRandomQuestionsAsync(string surveyId, int count)
        {
            var questions = await _questionRepository.GetRandomBalancedQuestionsAsync(surveyId, count);
            var dtoList = _mapper.Map<List<SurveyQuestionDto>>(questions);

            return ServiceResponse<List<SurveyQuestionDto>>.SuccessResponse(dtoList);
        }
        public async Task<ServiceResponse<List<QuestionDto>>> GetQuestionsByQuizIdAsync(string quizId)
        {
            var questions = await _questionRepository.GetQuestionsByQuizIdAsync(quizId);
            var dtos = _mapper.Map<List<QuestionDto>>(questions);
            return ServiceResponse<List<QuestionDto>>.SuccessResponse(dtos);
        }



    }
}
