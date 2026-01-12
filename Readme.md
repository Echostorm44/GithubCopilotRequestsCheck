Github Copilot Requests Check
A plugin for the [Flow launcher](https://github.com/Flow-Launcher/Flow.Launcher).

<img width="775" height="215" alt="image" src="https://github.com/user-attachments/assets/5416dc0c-21e9-4177-94d2-a3540b05c327" />


If you have a subscription to Github Copilot and get much use out of it you know you have to keep an eye on your requests.  Instead of having to pop deep into settings or keep a tab opened I thought I'd use the Github API to give me a way to do a quick peek in Flow Launcher.  

You need a Personal Access Token which is easy to create and you can limit the scope to just being able to read your plan details which is pretty harmless. 

It is pretty easy.  Go to your settings in Github.  Then to Developer Settings 

<img width="215" height="229" alt="image" src="https://github.com/user-attachments/assets/de090c9e-4c2f-408b-bad7-4ba9ae742e37" />

Make a fine grained token

<img width="1119" height="211" alt="image" src="https://github.com/user-attachments/assets/49fce71f-e9db-472c-be67-beb2daae294a" />

And just add the Plan permission at Read-only access.

<img width="833" height="489" alt="image" src="https://github.com/user-attachments/assets/030539ed-7563-4d27-a0ff-cac3df1ad660" />


You can either keep that PAT in the ENV which is the most secure way to do it.  Just add a user scope key under GITHUB_COPILOT_PAT or you can add it via settings with your Github username and plan level / monthly request limit:

<img width="732" height="649" alt="image" src="https://github.com/user-attachments/assets/093fad92-0a28-4342-b40b-59b56d2e82dd" />

### Usage

    ghr
