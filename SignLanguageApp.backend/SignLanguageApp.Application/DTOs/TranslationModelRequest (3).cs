namespace SignLanguageApp.Application.DTOs;

/// <summary>Lo que se le manda a un modelo: frames[n][75][3].</summary>
public sealed record TranslationModelRequest(float[][][] Frames);
