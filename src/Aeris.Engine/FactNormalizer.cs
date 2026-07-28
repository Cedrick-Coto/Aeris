namespace Aeris.Engine;

public class FactNormalizer
{
    private readonly Dictionary<uint, string> _entityNames;

    public FactNormalizer(Dictionary<uint, string>? entityNames = null)
    {
        _entityNames = entityNames ?? new Dictionary<uint, string>();
    }

    public List<SemanticFact> Normalize(SemanticState state)
    {
        var facts = new List<SemanticFact>();

        AddIdentityFacts(state.Identity, facts);
        AddSituationFacts(state.Situation, facts);
        AddInternalFacts(state.Internal, facts);
        AddAttentionFacts(state.Attention, facts);
        AddWorkingMemoryFacts(state.WorkingMemory, facts);
        AddLongTermMemoryFacts(state.LongTermMemory, facts);
        AddWorldModelFacts(state.WorldModel, facts);
        AddSocialFacts(state.Social, facts);

        return facts;
    }

    private static void AddIdentityFacts(SemanticIdentity identity, List<SemanticFact> facts)
    {
        if (!string.IsNullOrEmpty(identity.Name) && !string.IsNullOrEmpty(identity.Species))
        {
            facts.Add(new SemanticFact
            {
                Subject = identity.Name,
                Predicate = "es",
                Object = $"{identity.Species}",
                Certainty = "seguro",
                Source = "identidad"
            });
        }

        if (!string.IsNullOrEmpty(identity.Name) && identity.AgeYears > 0)
        {
            facts.Add(new SemanticFact
            {
                Subject = identity.Name,
                Predicate = "tiene",
                Object = $"{identity.AgeYears} años",
                Certainty = "seguro",
                Source = "identidad"
            });
        }

        if (!string.IsNullOrEmpty(identity.Name) && !string.IsNullOrEmpty(identity.Role))
        {
            facts.Add(new SemanticFact
            {
                Subject = identity.Name,
                Predicate = "es",
                Object = identity.Role,
                Certainty = "seguro",
                Source = "identidad"
            });
        }
    }

    private static void AddSituationFacts(SemanticSituation situation, List<SemanticFact> facts)
    {
        if (!string.IsNullOrEmpty(situation.TimeOfDay))
        {
            var timeFact = situation.TimeOfDay switch
            {
                "Mañana" => "Es de mañana",
                "Tarde" => "Es de tarde",
                "Atardecer" => "Es atardecer",
                "Noche" => "Es de noche",
                _ => $"Son las {situation.TimeOfDay}"
            };
            facts.Add(new SemanticFact
            {
                Subject = "El mundo",
                Predicate = "está en",
                Object = timeFact.ToLower(),
                Certainty = "seguro",
                Source = "tiempo"
            });
        }

        if (!string.IsNullOrEmpty(situation.Season))
        {
            facts.Add(new SemanticFact
            {
                Subject = "La estación",
                Predicate = "es",
                Object = situation.Season.ToLower(),
                Certainty = "seguro",
                Source = "tiempo"
            });
        }

        if (!string.IsNullOrEmpty(situation.Weather))
        {
            facts.Add(new SemanticFact
            {
                Subject = "El clima",
                Predicate = "es",
                Object = situation.Weather.ToLower(),
                Certainty = "seguro",
                Source = "percepción"
            });
        }

        if (!string.IsNullOrEmpty(situation.CurrentActivity))
        {
            facts.Add(new SemanticFact
            {
                Subject = "La entidad activa",
                Predicate = "está",
                Object = situation.CurrentActivity.ToLower(),
                Certainty = "seguro",
                Source = "situación"
            });
        }
    }

    private static void AddInternalFacts(SemanticInternalState internalState, List<SemanticFact> facts)
    {
        if (!string.IsNullOrEmpty(internalState.PrimaryEmotion))
        {
            facts.Add(new SemanticFact
            {
                Subject = "La entidad activa",
                Predicate = "siente",
                Object = TranslateEmotion(internalState.PrimaryEmotion),
                Certainty = "alto",
                Source = "emoción"
            });
        }

        foreach (var goal in internalState.ActiveGoals)
        {
            if (!string.IsNullOrEmpty(goal.Description))
            {
                facts.Add(new SemanticFact
                {
                    Subject = "La entidad activa",
                    Predicate = "quiere",
                    Object = goal.Description.ToLower(),
                    Certainty = "alto",
                    Source = "objetivos"
                });
            }
        }

        if (!string.IsNullOrEmpty(internalState.PhysicalState))
        {
            facts.Add(new SemanticFact
            {
                Subject = "La entidad activa",
                Predicate = "está",
                Object = internalState.PhysicalState.ToLower(),
                Certainty = "medio",
                Source = "estado interno"
            });
        }

        if (!string.IsNullOrEmpty(internalState.MentalState))
        {
            facts.Add(new SemanticFact
            {
                Subject = "La entidad activa",
                Predicate = "está",
                Object = internalState.MentalState.ToLower(),
                Certainty = "medio",
                Source = "estado interno"
            });
        }
    }

    private static void AddAttentionFacts(SemanticAttention attention, List<SemanticFact> facts)
    {
        if (!string.IsNullOrEmpty(attention.PrimaryFocus) && attention.PrimaryFocus != "Ninguno")
        {
            facts.Add(new SemanticFact
            {
                Subject = "La entidad activa",
                Predicate = "está observando",
                Object = attention.PrimaryFocus.ToLower(),
                Certainty = "alto",
                Source = "atención"
            });
        }
    }

    private static void AddWorkingMemoryFacts(SemanticWorkingMemory wm, List<SemanticFact> facts)
    {
        foreach (var thought in wm.ActiveThoughts)
        {
            if (!string.IsNullOrEmpty(thought))
            {
                facts.Add(new SemanticFact
                {
                    Subject = "La entidad activa",
                    Predicate = "piensa en",
                    Object = thought.ToLower(),
                    Certainty = "medio",
                    Source = "memoria de trabajo"
                });
            }
        }

        foreach (var concern in wm.ImmediateConcerns)
        {
            if (!string.IsNullOrEmpty(concern))
            {
                facts.Add(new SemanticFact
                {
                    Subject = "La entidad activa",
                    Predicate = "le preocupa",
                    Object = concern.ToLower(),
                    Certainty = "medio",
                    Source = "memoria de trabajo"
                });
            }
        }
    }

    private static void AddLongTermMemoryFacts(SemanticLongTermMemory ltm, List<SemanticFact> facts)
    {
        foreach (var memory in ltm.Memories)
        {
            if (!string.IsNullOrEmpty(memory.Description))
            {
                facts.Add(new SemanticFact
                {
                    Subject = "La entidad activa",
                    Predicate = "recuerda",
                    Object = memory.Description.ToLower(),
                    Certainty = memory.Certainty?.ToLower() ?? "medio",
                    Source = "memoria"
                });
            }
        }
    }

    private static void AddWorldModelFacts(SemanticWorldModel wm, List<SemanticFact> facts)
    {
        foreach (var belief in wm.Beliefs)
        {
            if (!string.IsNullOrEmpty(belief.Statement))
            {
                facts.Add(new SemanticFact
                {
                    Subject = "La entidad activa",
                    Predicate = "cree que",
                    Object = belief.Statement.ToLower(),
                    Certainty = belief.Confidence?.ToLower() ?? "medio",
                    Source = "creencias"
                });
            }
        }

        foreach (var knowledge in wm.Knowledge)
        {
            if (!string.IsNullOrEmpty(knowledge.What))
            {
                facts.Add(new SemanticFact
                {
                    Subject = "La entidad activa",
                    Predicate = "sabe que",
                    Object = knowledge.What.ToLower(),
                    Certainty = knowledge.Certainty?.ToLower() ?? "medio",
                    Source = "conocimiento"
                });
            }
        }

        foreach (var threat in wm.Threats)
        {
            if (!string.IsNullOrEmpty(threat))
            {
                facts.Add(new SemanticFact
                {
                    Subject = "Existe una amenaza",
                    Predicate = "que es",
                    Object = threat.ToLower(),
                    Certainty = "medio",
                    Source = "modelo del mundo"
                });
            }
        }
    }

    private static void AddSocialFacts(SemanticSocialContext social, List<SemanticFact> facts)
    {
        foreach (var rel in social.Relationships)
        {
            if (!string.IsNullOrEmpty(rel.Name))
            {
                var relationType = string.IsNullOrEmpty(rel.Type) ? "conoce a" : $"tiene relación de {rel.Type.ToLower()} con";
                facts.Add(new SemanticFact
                {
                    Subject = "La entidad activa",
                    Predicate = relationType,
                    Object = rel.Name.ToLower(),
                    Certainty = "alto",
                    Source = "relaciones"
                });

                if (!string.IsNullOrEmpty(rel.TrustLevel) && rel.TrustLevel != "Media")
                {
                    facts.Add(new SemanticFact
                    {
                        Subject = "La entidad activa",
                        Predicate = $"confía {rel.TrustLevel.ToLower()} en",
                        Object = rel.Name.ToLower(),
                        Certainty = "medio",
                        Source = "relaciones"
                    });
                }
            }
        }

        if (!string.IsNullOrEmpty(social.SocialTension) && social.SocialTension != "Sin tensión detectada")
        {
            facts.Add(new SemanticFact
            {
                Subject = "La situación social",
                Predicate = "es",
                Object = social.SocialTension.ToLower(),
                Certainty = "medio",
                Source = "contexto social"
            });
        }
    }

    private static string TranslateEmotion(string emotion)
    {
        return emotion.ToLower() switch
        {
            "joy" => "alegría",
            "trust" => "confianza",
            "affection" => "afecto",
            "excitement" => "emoción",
            "pride" => "orgullo",
            "relief" => "alivio",
            "gratitude" => "gratitud",
            "fear" => "miedo",
            "anger" => "ira",
            "sadness" => "tristeza",
            "disgust" => "asco",
            "shame" => "vergüenza",
            "guilt" => "culpa",
            "jealousy" => "celos",
            "curiosity" => "curiosidad",
            "surprise" => "sorpresa",
            "confusion" => "confusión",
            "anticipation" => "anticipación",
            "boredom" => "aburrimiento",
            "fatigue" => "cansancio",
            "nostalgia" => "nostalgia",
            "melancholy" => "melancolía",
            "hope" => "esperanza",
            "despair" => "desesperación",
            "determination" => "determinación",
            "ambivalence" => "ambivalencia",
            _ => emotion.ToLower()
        };
    }
}
