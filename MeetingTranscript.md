Unknown Speaker  0:02  
Alrighty, that's

Speaker 1  0:08  
what's going to record everything that way I can use it later. All right, so, actually, probably best to start with Dan. So with this, what exactly should we be trying to I guess you say avoid when it comes to proprietary, proprietary data. What are you going to be doing? So it's going to be similar to the whip app, to where it connects to the like my sequel server, okay, to store all its data, and then it's also gonna pull information off of the visual server, okay,

Unknown Speaker  0:51  
as long as it's going only from visual to that

Speaker 1  0:54  
correct, there's no writing going to visual at all, then that's fine.

Speaker 2  0:59  
Yep, as long as it's an hour our servers and not in the web. Yep, that'll be fine, okay,

Speaker 1  1:06  
all right. And as I stated just a little earlier, the app is going to be what I call module based. So like the material handlers are going to have their own module in the application. Set of texts will have their own that way. If setup text needs something changed, I don't have to reinvent the wheel. I just have to mess with their module and it's done.

Speaker 3  1:32  
Can you give me an example of a module?

Unknown Speaker  1:35  
Because perfect. So

Speaker 1  1:40  
I'm actually working on a receiving application right now, which allows me to repeat

Unknown Speaker  1:49  
SIG. Hold on here. It will

Unknown Speaker  2:04  
again, as material comes in this

Unknown Speaker  2:08  
damn this laptop sucks

Speaker 1  2:12  
half the buttons don't like you have to jam them to make them work.

Unknown Speaker  2:15  
We replaced the keyboard on that one already.

Unknown Speaker  2:17  
It's still doing it

Speaker 1  2:19  
all right, so I'll explain what modules are once this loads.

Speaker 2  2:29  
I have a feeling somebody spilled stuff in there. Again. Sorry. You. It.

Speaker 1  2:53  
So for this first part here, this is what you would call like a login module. It's its own base of code that is self reliant on itself. So if I mess with anything for production, it does not mess with this. I got to change one thing real fast, because I'm not on the work server. You

Unknown Speaker  3:36  
now too, I

Unknown Speaker  3:47  
so for like instance,

Speaker 1  3:52  
the operator, everything they everything they see, it is its own module. They don't have access to what Todd Doyle have access to if you asked me to edit something that pertains to an operator for what they see, I can do that without affecting them. That's pretty much what a module is. It's okay. So like, here, just minimize this so it's easier to see. So like, here we have, like, receiving labels, dunnage labels, UPS. Each one of these is its own module. If I wanted to change something in here,

Speaker 1  4:34  
it would not affect anything for the dunnage label ups or anything else. And by the way, like the look of this is going to be, it's going to be the same framework, so the wait list will kind of look the same way,

Unknown Speaker  4:51  
all right. And then also another thing,

Speaker 1  4:57  
this guided wizard. This is also something I was thinking about implementing for the operators. So instead of having them struggle to figure out what they need, they would click, like a new button, click wizard, and then it would go step by step. So like, here I'm going to say I want a box.

Unknown Speaker  5:18  
Tell me what kind

Speaker 1  5:21  
I'll hit next. I put in the quantity next. And obviously this is receiving related information, but I would put an appeal in the where I'm putting it, and then save it. And then when saving it, it would shoot it to

Unknown Speaker  5:44  
the wait list, and then they would see it.

Speaker 3  5:46  
So would we have access to the wait list as well as leads?

Unknown Speaker  5:50  
Yes, you would actually have your own module.

Speaker 1  5:54  
You would actually you would be able to everybody will still be able to add things to the wait list, but you will have what I'm looking for, analytics rights, so you'll be able to actually see what your operators are putting on. So you have they're abusing it, such whatnot as where a normal operator, they would not have that right.

Unknown Speaker  6:17  
So they can't see the waitlist. No,

Speaker 1  6:19  
they can see the waitlist, but they cannot view

Speaker 2  6:23  
how long it took each person to do this, yes, but they can still see

Speaker 1  6:30  
correct and with that, with like for you, this is where the modules come into play. Say you came to me and you said, okay, not really related to the wait list, but I want to see what. I want to see what operators are currently logged into a job. If you came up to me and I said, I can add that to your module, and that would be only something that production leads would have access to. For Cody, like, if he wanted to add notes for a certain die, I could add it to his module, so on, so forth. So does that answer your question? Yeah, I'm not going to get into the coding aspect of it, but yeah, okay with that said, is, I mean, is there anything that anybody really wanted that they could think of Cody, what I showed you how you would enter dunnage and stuff like that. That's still going to be in play, because visual still does not store that. There is a way I can pull it from the notes, but if it's not written in a specific format, it won't work. So, I mean, other than having Brett nick or Chris Go and tell our I don't know who deals with it, but hey, go into every single job and put our done it into a user field. It's not going to happen. So that is the one thing that we will have to deal with, it's still doing that. And then, as most some of you've seen with the wait list now, I mean, obviously you can see it talks to visual so the workflow would still be the same for the operator. They put in their work order number that they have, they'd have to still enter the OP that they're working on, just because that's the way the work orders work actually, wait, never mind. I take that back, because you would enter the OP when you set up the job, and then all they have to do is enter what they need. That is right. So, yeah, they would just have their press number. They'd click it, because you already put the information in when you set up the job, the app would know exactly what it's running, and they could just click what they need. It'll get it right to the material handlers. Now, from the material handling aspect. Doyle Todd, is there anything? What would you change about tables ready?

Speaker 4  9:07  
Is it going to have a recents on it where they don't have to go through correct because we got press operators are not going to be tech savvy. We have to understand that. We have to be very simplified so we can keep the ball rolling. Are we still gonna be able to have a reasons where they can go in and click on it and put it back as a new

Speaker 1  9:28  
Yes, yes. Is the time gonna start over though Correct? Yes, yeah. It'll be like a trying to think of the best way to say it, like a favorites tab, like things that just that they use the most. They can hit favorites, click it done. All right, I just

Speaker 5  9:52  
didn't want to see you something recent come up from like five, six hours ago, right on

Speaker 4  9:57  
a new waitlist. Is it going to have the option? Okay? Example, like, I think I showed Monica this, but okay, they're asking for skids, then they're asking for Gaylords. A lot of times, what they'll do is they'll do pick up skids Gaylords all on one line, instead of three different three separate transactions, because if you do it as three separate transactions, you actually have to x that one transaction out, and then still stays on the wait list, correct? So, yeah, I could, you know where I'm referring to it, yes.

Speaker 1  10:31  
So the way I had the old one set up, I could keep it that way, is their list of things they can ask for will pop up, such as skids, Gaylords, whatnot, and they'll hit check marks on each one, and when they hit send, it will actually send each of those things as a different transaction. Okay?

Speaker 4  10:48  
Because what we are, what is happening on both ends is because we have a misunderstanding, you know, I mean, from the press operators to the material handlers, is we go do one thing, and they forget to do the other thing, and then they take it off, and then we have a delay in presses being functional and running.

Unknown Speaker  11:08  
Another thing I've been hearing,

Speaker 1  11:12  
hate to say from the mostly from the material hands that don't want to do anything, is all people are hand picking tasks. They're not grabbing coils. They're not grabbing dies. Is that something you I was thinking a way we could eliminate that is by having an auto assign so you finish a job. If there's something in the red you it's yours. You don't have a choice

Speaker 6  11:39  
if you do it by location. I mean, everyone should somebody from going from one side of the plan all the other side of the plan.

Speaker 1  11:46  
That was another thing I was thinking of, and this was brought up in the past. I think we've done it in the past. Is where you delegate material handlers to locations. Yeah, right. Zone people, zone people that I can set up too, to where Todd comes in in the day, and he's like, okay, employee a, you're in Zone A, employee B, you're in Zone B. And then the app automatically gives them, you know, if they need an auto assigned task, it'll auto assign a task to that location. If there is none for that location, well, then they're moving somewhere else. And that's something that Todd and Doyle and Matt can sit and talk about, I mean, if they want to go that route, that would be the material handling module. It's your wait list. I'm just putting it together. Okay, another thing I had, and they didn't show me.

Speaker 2  12:42  
So then at the beginning of the Todd shift, he'd have to go in, and they would just be zone A, B, C, and he would just assign in there, and then the computer would automatically

Unknown Speaker  12:51  
correct, okay.

Speaker 4  12:54  
The only thing I really say negative in this aspect is, when we start doing overlaps, we're gonna have the tendency where there's gonna be 1520 things on the wait list. If one person is gonna be assigned to one thing, it's just gonna keep accumulating, accumulating, accumulating.

Speaker 1  13:10  
But with that, if there's 10 to 20 things on the wait list, and their zone isn't being affected by those 20 things, and the app will see that, and it will assign them elsewhere. It's not going to strictly place them there. It's just if something hot comes up in their area, or if something goes into the red in their area, it's going to auto assign it.

Unknown Speaker  13:29  
How does it go red? Time?

Speaker 1  13:31  
So say a task is given, say a coil is given 20 minutes to get done. It hits the five minute marker, it goes into red, get it done, kind of thing. And then that way, if somebody is assigned to that zone, the second they get done with the current task, they're getting auto assigned a coil because it's in their zone it needs to get done. Now that's just an idea, like I said. It's something they would have them too. Would have to talk about, actually, know you as well, because your input would be good on that as well. Another thought I had was with quality, and this has nothing to do with any of you, honestly, but because I can go module with it, say an operator has bad parts, what I want to know is, how does quality know? Does the operator have to leave the press, go to quality, talk to somebody there and then go back? So why not just make a module in the wait list where, when an operator, they can add a quality task, material handlers won't see it. But when somebody who is a quality technician opens the wait list, they'll see their wait list, and then all of a sudden, be like, 14 has bad parts. Okay, go over there.

Speaker 4  14:49  
You got that problem right now with the spot welding department, the John Deere doors, every single Gaylord has to be reworked.

Unknown Speaker  14:59  
So how would quality? No,

Speaker 1  15:02  
they would just have to have they would have to have it open. Just like in the yellow handlers have it open.

Speaker 2  15:07  
They have that. Here's, here's my question. If you put a quality, I don't know if you want to call it a quality alert or flag whatever, can we have it that it automatically emails the quality email?

Speaker 1  15:20  
That is something I would have to get with you with because that is something I tried in the past with that app I just showed you, because I send out those receiving reports every day. Yeah, and I wanted that app I made to do it for me. It will generate the report for me, but I'm having trouble getting it to actually email because of security. Okay?

Speaker 7  15:41  
So you probably get quality port 80, that's like universal, you can just get it

Speaker 1  15:47  
passed for that. Okay, yeah, that's something we'll have to get together and talk

Speaker 2  15:50  
about quality. There's a email quality at or I remember if

Speaker 3  15:54  
it's quality, I just feel like, if it's something in the now that we need attention from quality, they're not going to read their emails, even if you know what. I mean, it's not going to we need someone now.

Speaker 1  16:06  
I mean, not now. This is out there. But if there was a way to have the app talk to the intercom, they send in a quality alert, and it just goes over the intercom. Quality requested at press 14, whatever, which, of course, somebody's gonna have to say, record all those messages.

Speaker 8  16:30  
The office personnel might have some questions about listening to those all day long. That's true.

Speaker 1  16:37  
Well, then, then again. I mean, that's something, that's something for you guys to think about. If you guys can think of

Speaker 5  16:42  
a game plan, or if it go right to their phones and page and rate at their phones that,

Speaker 1  16:47  
yeah, big flashing problem with that, though, is anything going to the phone violates it being in house, yeah, we cannot have phone apps that's already that's per Max and Nick

Speaker 4  17:02  
you need, I feel you need to do a little more research on and get more people involved

Speaker 1  17:08  
in it. Oh, absolutely. It was actually just an idea I had this morning.

Speaker 2  17:11  
Is there a way to send out a team's message to the quality department?

Speaker 3  17:17  
They don't even look at that either. It's more of talking with quality and getting a game plan with quality, and what's going to work best with quality.

Speaker 1  17:24  
Yeah, I talked to Austin this morning to see if he could be in, and he said, Well, I don't know. We'll see.

Speaker 3  17:29  
Yeah, it's because they really don't look at their teams either. We physically have to go in there and get, you know, get in front of someone.

Speaker 1  17:38  
Okay, that'd have been nice to have Nick or Kristen here on because then he could lit a fire under their ass.

Unknown Speaker  17:43  
Well, you have a new person in charge. That's true Exactly.

Speaker 1  17:47  
Yeah, talk to them. Yeah. They told me to have her in it today, but it's like she doesn't know really much about wait lists. So I think I'll get with her afterwards, because I just I just, I don't want to overwhelm her, because I'm sure she's got data overload right now,

Unknown Speaker  18:06  
or information overload.

Unknown Speaker  18:09  
Brett, you've been quiet. Well, it seems

Unknown Speaker  18:11  
like everybody here is on step 78 I'm on step one,

Speaker 1  18:16  
just right now. This is just an information gathering like, I want to know what everybody wants. And then what I'm going to do with this information is I'm just going to compile it all into a, what's called a mock up. Why are we getting order to the wait list? Because the wait list, okay, for instance, operator on 14 puts in for a coil. They read it wrong. They put it in. Doyle goes, gets to coil. He spends 25 No, he spends an hour digging it out, grabbing it, flipping it, bringing it there, and the whole time they put the wrong coil on the wait list. Yeah, that's an hour of wasted work because somebody can't read.

Speaker 4  18:53  
Now, if you have does that happen a lot? Yeah, it happens with parts too. Because what they're doing is they look at the part number, but they don't look at dumb at the bottom parts needed,

Speaker 5  19:04  
especially like, What whip Yeah, parts they don't put in the right Ops they don't put in Yeah, yes.

Speaker 3  19:10  
So they're dumb. If I not simplify that,

Speaker 1  19:15  
my goal is to make it to where the operator does not type the damn thing

Unknown Speaker  19:20  
unless they absolutely have to, they click from drop

Speaker 9  19:23  
down menu. There are times you need it. Well, I need this specifically, and it's not an option. I need to type something,

Unknown Speaker  19:28  
yep, it will have that Yes.

Speaker 1  19:32  
But 90% of the time I need a part,

Unknown Speaker  19:38  
or need pieces that are finished product picked up. Yep.

Speaker 4  19:42  
So is the mentor going to be training the people on the wait list, the new press operators?

Unknown Speaker  19:49  
So how would you want to handle that?

Speaker 4  19:53  
What was the question? Are we going to have the mentors training the press operators when we do go to this new format so they fully understand what's going on.

Speaker 1  20:03  
I mean, obviously I would have to train them first, and then they would have to, I mean, that'd be another thing too. We'd have to come up with the training programs.

Speaker 3  20:10  
Yeah, I was gonna say, I feel like this is a whole nother training thing, not just a checklist on the TRL, this is more in depth,

Speaker 1  20:18  
I think, where it would have to be like a like quality does their training? You know, I would have to set up time to do training with people, because we've all been

Speaker 4  20:26  
here when we first started here. It's like, you stand here and look like, what am I supposed to do? What I was going into, what I would ask for parts. We need to do this. You need to do that. Then you got two, three different, yeah, it's, we don't want

Speaker 1  20:39  
that exactly. And I mean, because it can talk to visual, I can make it do more. I mean, I can make it do a lot more than just the wait list. Like, if an operator wants to see the work order in a dumb, down version, it can do that too. Because, I mean, obviously the first time I ever looked at one of our work orders, I was baffled. I had no idea how to read the dang thing. Yeah, but if I could break it down into press part ID list of dunnage job specs, right? You know, they have that right there. It's simple. It's stupid, perfect. But that's far and beyond the point. It's something you know, once again, that whole module thing, you know, I want this later. We can add it later.

Unknown Speaker  21:26  
Todd, wake up, when is

Unknown Speaker  21:28  
this? Talking about this is gonna,

Unknown Speaker  21:30  
this isn't gonna be something that rolls out tomorrow. Yep, next

Unknown Speaker  21:37  
week. Change it. Okay, we got to change it.

Speaker 1  21:39  
No, I have already, I have already been told, told by Nick, by Chris, like Dan kind of mentioned it too the wait list. I am not allowed to roll out updates without their consent, and it will not be released until all hands on deck approved, like version 1.0 well. And that's, that's another thing too. The very first version that rolls out is going to be the closest thing the tables ready as we can get. All these ideas are things that will come after, and that's per Nick That's what he's wanted from the beginning. So the main goal, obviously, it's going to be the integration with visual to dumb it down and then get it on the press floor. Any ideas, any simplifications you want after that,

Unknown Speaker  22:33  
that way I have the information now I can start planning ahead.

Speaker 3  22:38  
So with this new this new wait list, operators will not be able to adjust

Unknown Speaker  22:43  
times. No.

Speaker 1  22:46  
So the way the times will work is every job type will have a preset time, and they will be adjusted by either Brett or Nick. That's it. They're the only two that can adjust them. And the way, if I remember right, the way Nick said he wanted to go about doing it, is we'll just do a default time for probably, like, the first few months, and then he's going to take that data that comes back from how long it took a material handler to do it, and then how, that's how he's going to set his average time.

Speaker 2  23:17  
So if it took him 10 minutes, he's going to dump it down to five.

Speaker 4  23:20  
Yeah, yeah. Because last night it came in, there was flat stock on there for two and a quarter hours.

Speaker 1  23:28  
Well, you know something, they're getting off subject, but assemblies going away. They're going to bits that whole area, but they want to put presses over there. We need flat stock space? No, we, but that's not the point. I'm just saying we need flat we need flat stock off the floor. We need more of those racks at

Speaker 4  23:48  
a lot of they are like they are getting them, but

Speaker 2  23:53  
hold right now. So how will the wait list affect bits versus this building? That's good question, because of it. You know, as we get more busses over there, so is able to keep it separated,

Speaker 1  24:06  
yep, so each building will have its own site ID, you know, visual has that what we don't utilize it. So like with that, at that app I just rolled out, I'm not going to open again, because it takes forever when you log into that app before anything. If it does not notice you as a user, it prompts you to add yourself. What I can do is when you add yourself as a user depending on what computer you're on. Actually, never mind Scratch that, when the computer runs the application based on what computer it is, it's going to automatically set the site ID, and that's the information it will show.

Speaker 2  24:48  
Okay? Because, well, all of those are virtual, but we know where they're located right now, right?

Speaker 1  24:55  
Okay, so, yeah, so that's one thing. I mean, down the road, that's one thing I'll need for me. I'll just need a list of all the and it probably be best just to have the IP addresses, not the computer names, because it's gonna Yeah, you're

Unknown Speaker  25:05  
gonna want to buy. You're not gonna want Yep.

Speaker 6  25:09  
So when we start a work order here and finish it over there, shouldn't matter. No.

Speaker 4  25:16  
Bad part is right now we have parts that are being ran in spot welding, where we have met DC right now, and they should be over here when somebody calls for parts, it's gonna show a DC location, you know what I mean. So we're gonna have to take the time to drive all the way over here and then bring them back.

Speaker 1  25:37  
That comes. So I do now, do they have to be at DC, or were they accidentally taken over there?

Speaker 4  25:43  
Accidentally taken over because a lot of times with as far as the forklift material handlers, they don't know what is right in house and what is going to this.

Speaker 1  25:54  
So what I could do is this would be on the setup when they're doing the setup part, they would have to look at where that part, where the finished good, or where the whip item goes next. Does it go here? Does it go to this? And they will just hit a checkbox. Then, when your guy picks it up, if, when they pick it up, it will just be like a notification somewhere on the screen that says, this item, Expo item. I think that

Speaker 10  26:24  
was cool. Does the truck, I remember, have a list of stuff that he's gonna get moved? What's that? What about the truck driver?

Speaker 2  26:31  
Can put him in there for a list of stuff that get moved? So he, he knows he has to go get stuff.

Speaker 4  26:37  
Well, see, a lot of times we don't know that. I don't know how you

Speaker 2  26:40  
incorporate it and what he's doing, but email,

Speaker 4  26:44  
we do have van method. A lot of times it'll be like, two, three skids.

Speaker 1  26:48  
Oh, yeah, no, I know I thought you're talking about our actual, like, semi truck drivers. You're saying, like, if, if you need Monica or somebody to get parts from, take parts to fits, or vice versa, that's what you're saying. Yeah.

Speaker 2  27:01  
Well, you were, you were saying that. But so how does it, how does it get checked off? Because material owners don't usually take the van, do they? Yeah, yeah.

Speaker 1  27:09  
Well, what we were talking about is material handlers will put items on the truck that don't belong on the truck. They belong here,

Speaker 4  27:19  
so apart and they think it's actually a finished part,

Unknown Speaker  27:23  
but that's also a that's also a good thought

Speaker 2  27:25  
too, though that could, that could come up with a red flag if you put something on the truck, Yes, yep, that doesn't belong on the truck. That would be nice too, then, because, yeah,

Speaker 1  27:34  
so it will still warn them. But if they go ahead and when they close the job or complete it. That's when it would flag.

Speaker 2  27:43  
Are we still using I don't know. Are we still using that when the material owners put stuff on the truck, it's to the truck location in the computer, yes, yes. Okay, so yeah, then it could, it could red flag and say, No, that doesn't go there, yep,

Unknown Speaker  27:56  
yeah. That's

Speaker 1  27:59  
nice. That would now, if it's a whip item, that would involve me also updating the whip app, because the whip apps gonna have to it.

Unknown Speaker  28:12  
Would, I wouldn't have a choice in the matter. Damn,

Speaker 4  28:19  
yeah, yeah. But I'm just thinking full spectrum here. You know what I mean? Because now we have to go over and get the parts. I mean, it's not it. It probably happens at least once or twice a day.

Speaker 1  28:32  
Yeah, for sure. Yep. Oh, when I was at vets, up all the time, these don't, these don't belong here. Put it back on the truck. Put it back in the truck. Yeah. See, that's what's

Speaker 4  28:40  
bad is because that's where the driver over there, Alan needs to or, I mean, he needs a little help on training wise, to be knowledgeable what has to go on and what doesn't. But also, it starts with us. Okay,

Unknown Speaker  28:58  
Doyle, you've been quiet.

Unknown Speaker  29:01  
Just take it all in.

Unknown Speaker  29:03  
You have no thoughts,

Speaker 5  29:05  
not right now. No, oh, wait, I do have one. Start running

Speaker 1  29:09  
material handlers when there isn't anything on the wait list, but yet they're doing stuff. How do you want them to still get credit for it? Because they're going around doing all these pickups that aren't on the wait list, Nick looks at the data, and he's not going to see that. Do you want, like, a Quick Add button? Like, okay, I did five pickups. We don't need that.

Speaker 5  29:31  
I want them guys to be like, old school, like, when I started here, when we didn't have computers on our forklifts, that that's not going to happen.

Speaker 1  29:40  
No, oh, I know what. You know. I'm saying, No, I'm but I'm saying, when Nick looks at it, yeah, when he looks at the data, or when, when a lead looks at the data,

Unknown Speaker  29:53  
say, Tim,

Speaker 5  29:55  
they should just know, because they know that press was running and nothing was put on the wait list because everything was taken care of.

Speaker 6  30:02  
So tier handlers have the ability to put something on the wait

Speaker 1  30:06  
list that that's what, that's what I was getting at, like a quick, like, Okay, I went to 14, did a pickup, quick ad, press 14, done. They get credit for it. And then also, what that does is, because trailer,

Unknown Speaker  30:23  
what's that that's always gonna be on my shift?

Speaker 4  30:26  
What do you mean that?

Speaker 1  30:28  
That was another thing I was about to say projects. He sends Mike Sam's to go clean up flat stock. You log into your project, and when you're done, you log out that way. They know, okay. They start to learn it's same thing with digging out coils. Okay, I gotta dig out this coil. I'm gonna log into dig out coil. That way they start seeing, okay, coil digging out.

Speaker 2  30:53  
These guys are spending a third of their time digging coils out.

Speaker 1  30:58  
Yeah, because what one like with Nick, it's about data. He wants to see it. We can

Speaker 3  31:03  
pull that off. Is it just like a quick click for them,

Speaker 4  31:07  
okay, yes. For us, yeah. Like, when you add something to the wait list, we do the same thing too. Like, outside clean and backpack.

Speaker 7  31:18  
So if they checked it off and it's not complete. Can they bring it back, like the task,

Speaker 1  31:25  
like the accident, yeah, oh, we only got it partially done.

Unknown Speaker  31:30  
Well, projects are never really complete.

Speaker 2  31:35  
Last 10 minutes, it's got something on it.

Speaker 1  31:38  
They can add, they can add, they when they close it, they can add a note to say what they did. And then that way, Doyle, Todd, anybody can see it and be like, okay, you know, when Doyle's talking to Matt, or Todd's talking to Doyle, he could be like, okay, you know,

Unknown Speaker  31:53  
I'm kind of like, I'm thinking

Speaker 2  31:56  
some of those projects are more like saying, Well, I took out the trash, yeah, but there's more trash in the barrel, so when it gets full, I'll take care of it again. But it's not it never ends.

Speaker 4  32:05  
Oh, there was times where, like when Doyle was outside doing all the Generac stuff two days, three days, yeah, yeah.

Speaker 2  32:12  
It doesn't end. As soon as you get it done, still not done.

Speaker 1  32:17  
Something else comes in and gets placed and that way. And with that way, there's a data trail. That way. There's a data trail for all this back end stuff that they don't think that they have to do. Because when I was on the floor, that was always, oh, you you got, you got to stay on the floor. You got to stay on the floor. It's like, what this back end crap needs to get done? Or else the plants gonna look like a disaster, and look what happened. But the plant was the plant was a lot neater. When you had me and McKenna run around with chickens with like, like chickens with our heads cut off,

Unknown Speaker  32:49  
did look neater. Can't deny it.

Speaker 1  32:54  
Granted, the people on our shift didn't really care for it. The other material I know is because they had to work harder, but they didn't have to take out as many coils or

Speaker 5  33:05  
All right, I got to take a lap make sure this is all good, good to go.

Speaker 1  33:10  
So does anything have any anybody have anything else? Because I'm sure I've got like five trucks waiting on me. Nope, nope. Good. Great. Yeah. I.

Transcribed by https://otter.ai
