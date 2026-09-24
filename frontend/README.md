# MockInterviewPlatform

## Application structure

```text
src/app/
├── core/
│   ├── guards/
│   ├── models/
│   └── services/
├── features/
│   ├── interviewer/
│   │   ├── dashboard/pages/interviewer-dashboard/
│   │   ├── candidates/
│   │   │   ├── models/
│   │   │   ├── pages/         # candidate-list, candidate-details, candidate-form
│   │   │   └── services/
│   │   ├── interviews/
│   │   │   ├── models/
│   │   │   ├── pages/         # interview-list, interview-details, interview-form
│   │   │   └── services/
│   │   ├── feedback/          # models, pages, services
│   │   └── reports/           # pages, services
│   └── candidate/
│       ├── dashboard/pages/candidate-dashboard/
│       ├── interviews/pages/
│       │   ├── my-interviews/
│       │   └── my-interview-details/
│       ├── progress/pages/my-progress/
│       └── services/
├── layout/                   # header, sidebar, main-layout
└── app.routes.ts
```

`core` contains application-wide authentication, user models and route guards.
`features/interviewer` contains candidate management, interview management,
feedback and reports. `features/candidate` contains the signed-in candidate's
dashboard, interviews and progress. Its shared `CandidatePortalService` lives in
`candidate/services` because all three sections use the candidate API.

Keep feature-specific models and services with their feature. Put shared page
chrome in `layout`. Define navigation in `app.routes.ts`; folder names do not
change public URLs such as `/dashboard`, `/interviews` or `/my-interviews`.

This project was generated using [Angular CLI](https://github.com/angular/angular-cli) version 19.2.18.

## Development server

To start a local development server, run:

```bash
ng serve
```

Once the server is running, open your browser and navigate to `http://localhost:4200/`. The application will automatically reload whenever you modify any of the source files.

## Code scaffolding

Angular CLI includes powerful code scaffolding tools. To generate a new component, run:

```bash
ng generate component component-name
```

For a complete list of available schematics (such as `components`, `directives`, or `pipes`), run:

```bash
ng generate --help
```

## Building

To build the project run:

```bash
ng build
```

This will compile your project and store the build artifacts in the `dist/` directory. By default, the production build optimizes your application for performance and speed.

## Running unit tests

To execute unit tests with the [Karma](https://karma-runner.github.io) test runner, use the following command:

```bash
ng test
```

## Running end-to-end tests

For end-to-end (e2e) testing, run:

```bash
ng e2e
```

Angular CLI does not come with an end-to-end testing framework by default. You can choose one that suits your needs.

## Additional Resources

For more information on using the Angular CLI, including detailed command references, visit the [Angular CLI Overview and Command Reference](https://angular.dev/tools/cli) page.
