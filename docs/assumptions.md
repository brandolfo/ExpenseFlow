# Assumptions

| ID | Assumption | Why it matters | How to validate | Status |
| --- | --- | --- | --- | --- |
| A-001 | Users can provide CSV or Excel exports before PDF support. | This keeps the first workflow focused on structured data rather than difficult document parsing. | Ask Santiago which export formats he can access and create synthetic samples. | Open |
| A-002 | Categorization accuracy matters more than full automation. | The product should earn trust before optimizing for speed. | Test whether users prefer reviewable uncertainty over automatic but questionable labels. | Open |
| A-003 | Manual review is acceptable for ambiguous cases. | Human review supports trust and prevents AI from pretending to know. | Validate review tolerance with target users and measure how many cases are ambiguous. | Open |
| A-004 | Synthetic data is enough for public demos. | Public portfolio artifacts must avoid exposing real financial data. | Create realistic synthetic demo files and check whether they support the demo story. | Open |
| A-005 | A backend-first product can still be compelling with Swagger/API docs and generated reports. | The portfolio should showcase backend skill without needing a polished frontend first. | Review the demo with recruiters, peers, or interviewers and collect feedback. | Open |
