INSERT INTO public.genders (name)
VALUES ('Female')
ON CONFLICT (name) DO NOTHING;

INSERT INTO public.genders (name)
VALUES ('Male')
ON CONFLICT (name) DO NOTHING;

INSERT INTO public.genders (name)
VALUES ('Intersex')
ON CONFLICT (name) DO NOTHING;

INSERT INTO public.genders (name)
VALUES ('Trans')
ON CONFLICT (name) DO NOTHING;

INSERT INTO public.genders (name)
VALUES ('Non-Conforming')
ON CONFLICT (name) DO NOTHING;

INSERT INTO public.genders (name)
VALUES ('Personal')
ON CONFLICT (name) DO NOTHING;

INSERT INTO public.genders (name)
VALUES ('Eunuch')
ON CONFLICT (name) DO NOTHING;